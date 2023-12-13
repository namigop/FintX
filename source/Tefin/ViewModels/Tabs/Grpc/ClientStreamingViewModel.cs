#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingViewModel : GrpCallTypeViewModelBase {
    private string _statusText = "";
    private bool _showTreeEditor;

    public ClientStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ClientStreamingReqViewModel(mi, true);
        this.RespViewModel = new ClientStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StatusText = "";
        this.IsShowingRequestTreeEditor = true;
        this.ReqViewModel.SubscribeTo(x => ((ClientStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, vm => this.IsBusy = vm.IsBusy);
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
            this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.ReqViewModel.IsShowingClientStreamTree = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }
    public ClientStreamingReqViewModel ReqViewModel { get; }

    public ClientStreamingRespViewModel RespViewModel { get; }

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

    private void OnCallResponseChanged(ViewModelBase obj) {
        var reqVm = (ClientStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;

        if (resp.WriteCompleted) {
            async Task<object> CompleteRead() {
                var callResponse = await ClientStreamingResponse.getResponse(resp);
                return callResponse;
            }


            this.RespViewModel.Complete(resp.CallInfo.ResponseItemType, CompleteRead)
                .ContinueWith(t => {
                    if (t.Exception == null) {
                        var feature = new EndStreamingFeature();
                        feature.EndClientStreaming(resp);
                    }
                });
        }
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init();
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallClientStreamingFeature(mi, mParams, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();
                if (ok)
                    this.ReqViewModel.SetupClientStream((ClientStreamingCallResponse)response); //
            }
        }
        finally {
            this.IsBusy = false;
        }
    }
}