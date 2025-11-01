using System.Net;
using System.Threading;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Tefin.Grpc.Sample.Services;

namespace Tefin.Features;

public class StartDemoGrpcServiceFeature {
    private const string _clientName = "NorthwindServiceClient";
    public static string Url = "http://localhost:54321";
    private WebApplication? _app;
    private CancellationTokenSource? _cs;

    public bool IsStarted { get; private set; }

    public string GetClientName() => _clientName + "-" + Path.GetFileNameWithoutExtension(Path.GetTempFileName());

    public async Task Start() {
        this._cs = new CancellationTokenSource();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 54321));
        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        this._app = builder.Build();
        this._app.MapGrpcService<NorthwindService2>();
        this._app.MapGrpcReflectionService();
        this._app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        await this._app.StartAsync(this._cs.Token);
        this.IsStarted = true;
    }

    public async Task Stop() {
        if (this._cs == null) {
            return;
        }

        await this._app!.StopAsync(this._cs.Token);
        ;
        this._cs.Dispose();
        this._cs = null;
        this._app = null;
        this.IsStarted = false;
    }
}