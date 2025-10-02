using System.Reflection;
using System.Threading;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Tefin.Core.Interop;

namespace Tefin.Features;

public class ServerHost(Type serviceType, uint port, string serviceName) {
    private CancellationTokenSource? _csource;

    public async Task Stop() {
        await this._csource?.CancelAsync()!;
        this._csource?.Dispose();
        this._csource = null;
    }

    public async Task Start() {
        this._csource = new CancellationTokenSource();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP((int)port);
            serverOptions.ListenAnyIP((int)port + 1, listenOptions => {
                listenOptions.UseHttps();
            });
        });
        builder.Services.AddScoped(serviceType, sp => {
            var cons = serviceType.GetConstructors().First();
            var instance = cons.Invoke([serviceName]);
            //var instance = Activator.CreateInstance(serviceType,
            //    BindingFlags.CreateInstance, null, new[]{ serviceName })!;
            return instance;
        });
        
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();
        var app = builder.Build();
        
        var mapGrpcServiceMethod = typeof(GrpcEndpointRouteBuilderExtensions)
            .GetMethod("MapGrpcService")!
            .MakeGenericMethod(serviceType); 
        mapGrpcServiceMethod.Invoke(null, [app]);
        app.MapGrpcReflectionService();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await app.StartAsync(this._csource.Token);
    }
}