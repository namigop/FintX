#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingViewModel : GrpCallTypeViewModelBase {
    private string _statusText;
    private bool _showTreeEditor;

    public ServerStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ServerStreamingReqViewModel(mi, true);
        this.RespViewModel = new ServerStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this._statusText = "";
        this._showTreeEditor = true;
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
    }
    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }

    private async Task OnImportRequest() {
        await this.ReqViewModel.ImportRequest();
    }
    private async Task OnExportRequest() {
        await this.ReqViewModel.ExportRequest();
    }
    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref this._showTreeEditor , value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }
    public ServerStreamingReqViewModel ReqViewModel { get; set; }
    public ServerStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override void Init() {
        this.ReqViewModel.Init();
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init();
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallServerStreamingFeature(mi, mParams, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();
                this.IsBusy = false;

                this.RespViewModel.Show(ok, response, context);
                await this.RespViewModel.SetupServerStreamNode(response);
                await this.RespViewModel.Complete(typeof(StandardResponseViewModel.GrpcStandardResponse), CompleteRead);

                var elapsed = DateTime.Now - context.StartTime;
                this.StatusText = $"Elapsed: {printTimeSpan(elapsed)}";

                async Task<object> CompleteRead() {
                    var readServerStream = new ReadServerStreamFeature();
                    var callResponse = (ServerStreamingCallResponse)response;
                    callResponse = await readServerStream.CompleteRead(callResponse);
                    var end = new EndStreamingFeature();
                    callResponse = end.EndServerStreaming(callResponse);

                    var model = new StandardResponseViewModel.GrpcStandardResponse() {
                        Headers = callResponse.Headers.Value,
                        Trailers =   callResponse.Trailers.Value,
                        Status = callResponse.Status.Value
                    };
                    return model;
                }
            }
        }
        finally {
            this.IsBusy = false;
        }
    }
}