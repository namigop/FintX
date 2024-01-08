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

public class ClientStreamingViewModel : GrpCallTypeViewModelBase {
    private bool _showTreeEditor;
    private string _statusText = "";

    public ClientStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ClientStreamingReqViewModel(mi, true);
        this.RespViewModel = new ClientStreamingRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this.StatusText = "";
        this.IsShowingRequestTreeEditor = true;
        this.ReqViewModel.SubscribeTo(x => ((ClientStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, vm => this.IsBusy = vm.IsBusy);
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
        this.ReqViewModel.SubscribeTo(x => ((ClientStreamingReqViewModel)x).CanWrite, this.OnCanWriteChanged);
    }

    public ICommand StopCommand { get; }

    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }

    public bool CanStop {
        get => this.ReqViewModel is { CanWrite: true, RequestEditor.CtsReq: not null };
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

    public override string GetRequestContent() {
        return this.ReqViewModel.GetRequestContent();
    }

    private void OnCanWriteChanged(ViewModelBase obj) {
        this.RaisePropertyChanged(nameof(this.CanStop));
    }
    private void  OnStop() {
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

    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override void Init() {
        this.ReqViewModel.Init();
    }
    
    public override void ImportRequest(string requestFile) {
        this.ReqViewModel.ImportRequestFile(requestFile);
    }

    private async Task<object> EndClientStreamingCall(ClientStreamingCallResponse callResponse) {
        var builder = new CompositeResponseFeature();
        var (type, response) = await builder.BuildClientStreamResponse(callResponse);

        this.ReqViewModel.RequestEditor.EndRequest();
        this.RaisePropertyChanged(nameof(this.CanStop));
        return response;
    }

    private void OnCallResponseChanged(ViewModelBase obj) {
        var reqVm = (ClientStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;

        if (resp.WriteCompleted) {
            this.EndStreaming(resp);
        }
    }
    private void EndStreaming(ClientStreamingCallResponse resp) {
        async Task<object> CompleteRead() {
            //get the method call response
            var callResponse = await ClientStreamingResponse.getResponse(resp);

            //get the headers/trailers
            var feature = new EndStreamingFeature();
            callResponse = await feature.EndClientStreaming(callResponse); //TODO: use same emitted structure as UnaryAsync
            var response = await this.EndClientStreamingCall(callResponse);

            return response;
        }


        _ = this.RespViewModel.Complete(resp.CallInfo.ResponseItemType, CompleteRead);
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
                else {
                    this.EndStreaming((ClientStreamingCallResponse)response);
                }
            }
        }
        finally {
            this.IsBusy = false;
        }
    }
}