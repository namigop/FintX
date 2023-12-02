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

public class ClientStreamingViewModel : GrpCallTypeViewModelBase {
    private string _statusText;

    public ClientStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ClientStreamingReqViewModel(mi, true);
        this.RespViewModel = new ClientStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StatusText = "";

        this.ReqViewModel.SubscribeTo(x => ((ClientStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
    }

    public ClientStreamingCallResponse CallResponse => this.ReqViewModel.CallResponse;

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

        async Task<object> CompleteRead() {
            var stdResponse = ClientStreamingResponse.toStandardCallResponse(resp);
            if (resp.HasStatus) return await ClientStreamingResponse.getResponse(resp);

            return null;
        }

        _ = this.RespViewModel.Complete(resp.CallInfo.ResponseItemType, CompleteRead);
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            var mi = this.ReqViewModel.MethodInfo;
            var mParams = this.ReqViewModel.GetMethodParameters();
            var cfg = new CallConfig("http://localhost:5000", false, "", none<Cert>(), this.Io);

            var feature = new CallClientStreamingFeature(mi, mParams, cfg, this.Io);
            var (ok, resp) = await feature.Run();
            var (_, response, context) = resp.OkayOrFailed();
            if (ok) this.ReqViewModel.SetupClientStream((ClientStreamingCallResponse)response);
            this.IsBusy = false;
        }
        finally {
            this.IsBusy = false;
        }
    }
}