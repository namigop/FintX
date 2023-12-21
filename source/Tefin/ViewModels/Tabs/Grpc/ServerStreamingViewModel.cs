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
    private bool _showTreeEditor;
    private string _statusText;

    public ServerStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ServerStreamingReqViewModel(mi, true);
        this.RespViewModel = new ServerStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this._statusText = "";
        this._showTreeEditor = true;
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
        this.RespViewModel.SubscribeTo(x => ((ServerStreamingRespViewModel)x).CanRead, this.OnCanReadChanged);
    }

    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }
    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }
    public ServerStreamingReqViewModel ReqViewModel { get; set; }
    public ServerStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public bool CanStop {
        get => this.RespViewModel.CanRead && this.ReqViewModel.RequestEditor.CtsReq != null;
    }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    private void OnCanReadChanged(ViewModelBase obj) {
        this.RaisePropertyChanged(nameof(this.CanStop));
    }

    private async Task OnImportRequest() {
        await this.ReqViewModel.ImportRequest();
    }
    private async Task OnExportRequest() {
        await this.ReqViewModel.ExportRequest();
    }

    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override void Init() {
        this.ReqViewModel.Init();
    }
    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
        }
    }



    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init();
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                this.ReqViewModel.RequestEditor.StartRequest();

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
                    callResponse = await end.EndServerStreaming(callResponse);

                    var model = new StandardResponseViewModel.GrpcStandardResponse {
                        Headers = callResponse.Headers.Value,
                        Trailers = callResponse.Trailers.Value,
                        Status = callResponse.Status.Value
                    };
                    return model;
                }
            }
        }
        finally {
            this.IsBusy = false;
            this.ReqViewModel.RequestEditor.EndRequest();
            //this.RaisePropertyChanged(nameof(this.CanStop));
        }
    }
}