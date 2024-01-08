#region

using System.Reactive;
using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingViewModel : GrpCallTypeViewModelBase {
    private bool _showTreeEditor;
    private string _statusText;

    public DuplexStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new DuplexStreamingReqViewModel(mi, true);
        this.RespViewModel = new DuplexStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this._statusText = "";
        this._showTreeEditor = true;

        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, this.OnIsBusyChanged);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).IsBusy, this.OnIsBusyChanged);
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CanWrite, _ => this.RaisePropertyChanged(nameof(this.CanStart)));
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).CanRead, _ => this.RaisePropertyChanged(nameof(this.CanStart)));
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CanWrite, this.OnCanWriteChanged);
    }
    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }
    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.ReqViewModel.IsShowingClientStreamTree = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
            this.RespViewModel.IsShowingServerStreamTree = value;
        }
    }
    public bool CanStop {
        get => this.ReqViewModel.CanWrite && this.ReqViewModel.RequestEditor.CtsReq != null;
    }
    public DuplexStreamingReqViewModel ReqViewModel { get; }
    public DuplexStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public bool CanStart {
        get => !(this.ReqViewModel.CanWrite || this.RespViewModel.CanRead);
    }

    private void OnCanWriteChanged(ViewModelBase obj) {
        this.RaisePropertyChanged(nameof(this.CanStop));
    }
    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
            this.ReqViewModel.EndWriteCommand.Execute(Unit.Default);
            //await this.EndClientStreamingCall(this.ReqViewModel.CallResponse);
        }
    }

    private async Task OnImportRequest() {
        await this.ReqViewModel.ImportRequest();
    }
    private async Task OnExportRequest() {
        await this.ReqViewModel.ExportRequest();
    }

    public override string GetRequestContent() {
        return this.ReqViewModel.GetRequestContent();
    }
    public override void ImportRequest(string requestFile) {
        throw new NotImplementedException();
    }
    private void OnIsBusyChanged(ViewModelBase obj) {
        this.IsBusy = obj.IsBusy;
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
            this.EndStreaming(resp);
        }
    }
    private void EndStreaming(DuplexStreamingCallResponse resp) {
        async Task<object> CompleteRead() {
            var feature = new EndStreamingFeature();
            var respWithStatus = await feature.EndDuplexStreaming(resp);

            object model = new StandardResponseViewModel.GrpcStandardResponse {
                Headers = respWithStatus.Headers.Value,
                Trailers = respWithStatus.Trailers.Value,
                Status = respWithStatus.Status.Value
            };

            this.ReqViewModel.RequestEditor.EndRequest();
            this.RaisePropertyChanged(nameof(this.CanStop));
            return model;
        }

        _ = this.RespViewModel.Complete(typeof(StandardResponseViewModel.GrpcStandardResponse), CompleteRead);
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
                else {
                    var d = (DuplexStreamingCallResponse)response;
                    d = await DuplexStreamingResponse.getResponseHeader(d);
                    this.EndStreaming(d);
                }
            }
        }
        finally {
            this.IsBusy = false;
        }
    }
}