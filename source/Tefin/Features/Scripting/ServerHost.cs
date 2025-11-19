using System.Threading;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Tefin.Features.Scripting;

public class ServerHost(Type serviceType, 
    uint port, 
    string serviceName,
    bool useNamedPipes,
    string pipeName,
    bool useUnixDomainSockets,
    string socketFilePath) {
    
    private WebApplication? _app;
    private CancellationTokenSource? _csource;
    public bool IsRunning { get; private set; }

    public async Task Start() {
        try {
            this._csource = new CancellationTokenSource();
            var builder = WebApplication.CreateBuilder();
            if (useNamedPipes) {
                builder.WebHost.ConfigureKestrel(serverOptions => {
                    serverOptions.ListenNamedPipe(pipeName, listenOptions => {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                });
            }
            else if (useUnixDomainSockets) {
                if (File.Exists(socketFilePath))
                    File.Delete(socketFilePath);
                builder.WebHost.ConfigureKestrel(serverOptions => {
                    serverOptions.ListenUnixSocket(socketFilePath, listenOptions => {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                });
            }
            else {
                builder.WebHost.ConfigureKestrel(serverOptions => {
                    serverOptions.ListenAnyIP((int)port);
                    serverOptions.ListenAnyIP((int)port + 1, listenOptions => {
                        listenOptions.Protocols = HttpProtocols.Http2;
                        listenOptions.UseHttps();
                    });
                });
            }
            
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

            this._app = app;
            await app.StartAsync(this._csource.Token);
            this.IsRunning = true;
        }
        catch (Exception e) {
            this.IsRunning = false;
            throw;
        }
    }

    public async Task Stop() {
        if (this._csource == null) {
            this.IsRunning = false;
            return;
        }

        await this._app?.StopAsync(this._csource!.Token)!;
        this._csource?.Dispose();
        this._csource = null;
        this._app = null;
        this.IsRunning = false;

        if (useUnixDomainSockets && File.Exists(socketFilePath)) {
            File.Delete(socketFilePath);
        }
    }
}