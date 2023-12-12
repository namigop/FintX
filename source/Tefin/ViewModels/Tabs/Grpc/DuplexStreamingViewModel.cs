#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingViewModel : GrpCallTypeViewModelBase {
    private string _statusText;
    private bool _showTreeEditor;

    public DuplexStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new DuplexStreamingReqViewModel(mi, true);
        this.RespViewModel = new DuplexStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this._statusText = "";
        this._showTreeEditor = true;

        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, this.OnIsBusyChanged);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).IsBusy, this.OnIsBusyChanged);
    }

    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
    }
    public bool ShowTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref _showTreeEditor , value);
            this.ReqViewModel.ShowTreeEditor = value;
            this.ReqViewModel.IsShowingClientStreamTree = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }
    public DuplexStreamingReqViewModel ReqViewModel { get; }
    public DuplexStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public override void Init() {
        this.ReqViewModel.Init();
    }

    private void OnCallResponseChanged(ViewModelBase obj) {
        var reqVm = (DuplexStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;
        if (resp == null)
            return;
        
        if (resp.WriteCompleted) {
            Task<object> CompleteRead() {
                var feature = new EndStreamingFeature();
                var respWithStatus = feature.EndDuplexStreaming(resp);

                object model = new StandardResponseViewModel.GrpcStandardResponse() {
                    Headers = respWithStatus.Headers.Value,
                    Trailers = respWithStatus.Trailers.Value,
                    Status = respWithStatus.Status.Value
                };
                return Task.FromResult(model);
            }

            _ = this.RespViewModel.Complete(typeof(StandardResponseViewModel.GrpcStandardResponse), CompleteRead);
        }
    }

    private async Task OnStart() {
        try {
            this.IsBusy = true;
            this.RespViewModel.Init();
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallDuplexStreamingFeature(mi, mParams, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();
                if (ok) {
                    this.ReqViewModel.SetupDuplexStream((DuplexStreamingCallResponse)response);
                    this.RespViewModel.Show(ok, response, context);
                    _ = this.RespViewModel.SetupDuplexStreamNode(response);
                }
            }
        }
        finally {
            this.IsBusy = false;
        }
    }
}