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

public class DuplexStreamingViewModel : GrpCallTypeViewModelBase
{
    private string _statusText;

    public DuplexStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg)
    {
        this.ReqViewModel = new DuplexStreamingReqViewModel(mi, true);
        this.RespViewModel = new DuplexStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StatusText = "";

        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, OnIsBusyChanged);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).IsBusy, OnIsBusyChanged);
    }

    private void OnIsBusyChanged(ViewModelBase obj)
    {
        this.IsBusy = obj.IsBusy;
    }

    public DuplexStreamingCallResponse CallResponse => this.ReqViewModel.CallResponse;
    public DuplexStreamingReqViewModel ReqViewModel { get; }
    public DuplexStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText
    {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public override void Init()
    {
        this.ReqViewModel.Init();
    }

    private async void OnCallResponseChanged(ViewModelBase obj)
    {
        var reqVm = (DuplexStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;

        Task<object> CompleteRead()
        {
            object model = new StandardResponseViewModel.GrpcStandardResponse()
            {
                Headers = resp.Headers.Value,
                Trailers = resp.Trailers.Value,
                Status = resp.Status.Value
            };
            return Task.FromResult(model);
        }

        if (resp.HasStatus)
        {
            this.IsBusy = false;
            await this.RespViewModel.Complete(typeof(StandardResponseViewModel.GrpcStandardResponse), CompleteRead);
        }
    }

    private async Task OnStart() {
        try {
            this.IsBusy = true;

            var mi = this.ReqViewModel.MethodInfo;
            var mParams = this.ReqViewModel.GetMethodParameters();

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
        finally {
            this.IsBusy = false;
        }
    }
}