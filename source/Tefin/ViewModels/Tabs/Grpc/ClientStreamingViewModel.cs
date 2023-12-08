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

    public ClientStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ClientStreamingReqViewModel(mi, true);
        this.RespViewModel = new ClientStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StatusText = "";

        this.ReqViewModel.SubscribeTo(x => ((ClientStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, vm => this.IsBusy = vm.IsBusy);
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
            var mParams = this.ReqViewModel.GetMethodParameters();
            var clientConfig = this.Client.Config.Value;
            var feature = new CallClientStreamingFeature(mi, mParams, clientConfig, this.Io);
            var (ok, resp) = await feature.Run();
            var (_, response, context) = resp.OkayOrFailed();
            if (ok)
                this.ReqViewModel.SetupClientStream((ClientStreamingCallResponse)response); //

            this.IsBusy = false;
        }
        finally {
            this.IsBusy = false;
        }
    }
}