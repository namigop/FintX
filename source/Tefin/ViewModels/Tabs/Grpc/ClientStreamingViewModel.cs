#region

using System.Reactive;
using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingViewModel : GrpCallTypeViewModelBase {
    private bool _showTreeEditor;
    private string _statusText = "";

    public ClientStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ClientStreamingReqViewModel(mi, cg, true);
        this.RespViewModel = new ClientStreamingRespViewModel(mi, cg);
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

    public bool CanStop => this.ReqViewModel is { CanWrite: true, RequestEditor.CtsReq: not null };
    public override bool IsLoaded => this.ReqViewModel.IsLoaded;
    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }

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

    public ICommand StopCommand { get; }

    private readonly AllVariableDefinitions _envVars = new();
    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override string GetRequestContent() {
        var (ok, mParams) = this.ReqViewModel.GetMethodParameters();
        if (ok) {
            var (isValid, reqStream) = this.ReqViewModel.ClientStreamEditor.GetList();
            if (isValid) {
                var methodInfoNode = (MethodInfoNode)this.ReqViewModel.TreeEditor.Items[0];
                var requestVariables = methodInfoNode.Variables;

                List<VarDefinition> responseVariables;
                if (this.RespViewModel.TreeResponseEditor.Items.FirstOrDefault() is ResponseNode respNode) {
                    responseVariables = respNode.Variables;
                }
                else {
                    responseVariables = this._envVars.ResponseVariables;
                }
               
                List<VarDefinition> requestStreamVariables;
                if (this.ReqViewModel.ClientStreamTreeEditor.StreamItems.FirstOrDefault() is StreamNode rs) {
                    requestStreamVariables = rs.Variables;
                }
                else {
                    requestStreamVariables = this._envVars.RequestStreamVariables;
                }
                
                var feature = new ExportFeature(this.MethodInfo, mParams, requestVariables, responseVariables, requestStreamVariables, [], reqStream);
                var exportReqJson = feature.Export();
                if (exportReqJson.IsOk) {
                    return exportReqJson.ResultValue;
                }
            }
        }

        return string.Empty;
    }

   public override void ImportRequest(string requestFile) {
        GrpcUiUtils.ImportRequest(
            this.ReqViewModel.RequestEditor,
            this.ReqViewModel.ClientStreamEditor,
            this._envVars,
            this.ReqViewModel.ListType,
            this.MethodInfo,
            this.Client,
            requestFile,
            this.Io);
        
        this.ReqViewModel.RequestVariables = this._envVars.RequestVariables;
        this.ReqViewModel.RequestStreamVariables = this._envVars.RequestStreamVariables;
        this.ReqViewModel.IsLoaded = true;
    }

    public override void Init() {
        this.ReqViewModel.Init(this._envVars.RequestVariables);
    }

    private async Task<object> EndClientStreamingCall(ClientStreamingCallResponse callResponse) {
        var builder = new CompositeResponseFeature();
        var (type, response) = await builder.BuildClientStreamResponse(callResponse);

        this.ReqViewModel.RequestEditor.EndRequest();
        this.RaisePropertyChanged(nameof(this.CanStop));
        return response;
    }

    private void EndStreaming(ClientStreamingCallResponse resp) {
        async Task<object> CompleteRead() {
            //get the method call response
            var callResponse = await ClientStreamingResponse.getResponse(resp);

            //get the headers/trailers
            var feature = new EndStreamingFeature();
            callResponse = await feature.EndClientStreaming(callResponse);
            var response = await this.EndClientStreamingCall(callResponse);

            return response;
        }

        _ = this.RespViewModel.Complete(resp.CallInfo.ResponseItemType, CompleteRead);
    }

    private void OnCallResponseChanged(ViewModelBase obj) {
        var reqVm = (ClientStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;

        if (resp.WriteCompleted) {
            this.EndStreaming(resp);
        }
    }

    private void OnCanWriteChanged(ViewModelBase obj) => this.RaisePropertyChanged(nameof(this.CanStop));

    private async Task OnExportRequest() {
        var (ok, mParams) = this.ReqViewModel.GetMethodParameters();
        if (ok) {
            var (isValid, reqStream) = this.ReqViewModel.ClientStreamEditor.GetList();
            if (!isValid) {
                this.Io.Log.Warn("Request stream is invalid. Content will not be saved to the request file");
            }
            var methodInfoNode = (MethodInfoNode)this.ReqViewModel.TreeEditor.Items[0];
            var requestVariables = methodInfoNode.Variables;
               
            List<VarDefinition> responseVariables;
            if (this.RespViewModel.TreeResponseEditor.Items.FirstOrDefault() is ResponseNode respNode) {
                responseVariables = respNode.Variables;
            }
            else {
                responseVariables = this._envVars.ResponseVariables;
            }
            
            List<VarDefinition> requestStreamVariables;
            if (this.ReqViewModel.ClientStreamTreeEditor.StreamItems.FirstOrDefault() is StreamNode rs) {
                requestStreamVariables = rs.Variables;
            }
            else {
                requestStreamVariables = this._envVars.RequestStreamVariables;
            }

            await GrpcUiUtils.ExportRequest(
                mParams,
                requestVariables, 
                responseVariables,
                requestStreamVariables,
                [],
                reqStream, 
                this.MethodInfo,
                this.Io);
        }
    } //await this.ReqViewModel.ExportRequest();

    private async Task OnImportRequest() {
        await GrpcUiUtils.ImportRequest(
            this.ReqViewModel.RequestEditor,
            this.ReqViewModel.ClientStreamEditor,
            this._envVars,
            this.ReqViewModel.ListType,
            this.MethodInfo, 
            this.Client, 
            this.Io);

        this.ReqViewModel.RequestStreamVariables = this._envVars.RequestStreamVariables;
        this.ReqViewModel.RequestVariables = this._envVars.RequestVariables;
        this.ReqViewModel.IsLoaded = true;
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init(this._envVars);
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallClientStreamingFeature(mi, mParams, Current.EnvFilePath, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();
                if (ok) {
                    this.ReqViewModel.SetupClientStream((ClientStreamingCallResponse)response, this._envVars.RequestStreamVariables); //
                }
                else {
                    this.EndStreaming((ClientStreamingCallResponse)response);
                }
            }
        }
        finally {
            this.IsBusy = false;
        }
    }

    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
            this.ReqViewModel.EndWriteCommand.Execute(Unit.Default);
            //await this.EndClientStreamingCall(this.ReqViewModel.CallResponse);
        }
    }
}