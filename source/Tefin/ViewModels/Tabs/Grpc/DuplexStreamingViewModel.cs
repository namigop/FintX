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

public class DuplexStreamingViewModel : GrpCallTypeViewModelBase {
    private bool _showTreeEditor;
    private string _statusText;

    public DuplexStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new DuplexStreamingReqViewModel(mi, cg, true);
        this.RespViewModel = new DuplexStreamingRespViewModel(mi, cg);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this._statusText = "";
        this._showTreeEditor = true;

        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CallResponse, this.OnCallResponseChanged);
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).IsBusy, this.OnIsBusyChanged);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).IsBusy, this.OnIsBusyChanged);
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CanWrite,
            _ => this.RaisePropertyChanged(nameof(this.CanStart)));
        this.RespViewModel.SubscribeTo(x => ((DuplexStreamingRespViewModel)x).CanRead,
            _ => this.RaisePropertyChanged(nameof(this.CanStart)));
        this.ReqViewModel.SubscribeTo(x => ((DuplexStreamingReqViewModel)x).CanWrite, this.OnCanWriteChanged);
    }

    public bool CanStart => !(this.ReqViewModel.CanWrite || this.RespViewModel.CanRead);

    public override bool IsLoaded => this.ReqViewModel.IsLoaded;
    public bool CanStop => this.ReqViewModel.CanWrite && this.ReqViewModel.RequestEditor.CtsReq != null;

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

    public DuplexStreamingReqViewModel ReqViewModel { get; }
    public DuplexStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public ICommand StopCommand { get; }

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
                if (this.ReqViewModel.ClientStreamTreeEditor.StreamItems.FirstOrDefault() is ResponseStreamNode rs) {
                    requestStreamVariables = rs.Variables;
                }
                else {
                    requestStreamVariables = this._envVars.RequestStreamVariables;
                }
            
                List<VarDefinition> responseStreamVariables;
                if (this.RespViewModel.ServerStreamTreeEditor.StreamItems.FirstOrDefault() is ResponseStreamNode respStream) {
                    responseStreamVariables = respStream.Variables;
                }
                else {
                    responseStreamVariables = this._envVars.ResponseStreamVariables;
                }
                
                var feature = new ExportFeature(this.MethodInfo, mParams, requestVariables, responseVariables, requestStreamVariables, responseStreamVariables, reqStream);
                var exportReqJson = feature.Export();
                if (exportReqJson.IsOk) {
                    return exportReqJson.ResultValue;
                }
            }
        }

        return string.Empty;
    }

    public override void ImportRequest(string requestFile) {
      GrpcUiUtils.ImportRequest(this.ReqViewModel.RequestEditor,
              this.ReqViewModel.ClientStreamEditor, 
              this._envVars,
              this.ReqViewModel.ListType, 
              this.MethodInfo,
              this.Client,
              requestFile,
              this.Io);
      this.ReqViewModel.RequestStreamVariables = this._envVars.RequestStreamVariables;
      this.ReqViewModel.RequestVariables = this._envVars.RequestVariables;
      this.ReqViewModel.IsLoaded = true;
    } //this.ReqViewModel.ImportRequestFile(requestFile);

    public override void Init() => this.ReqViewModel.Init(this._envVars.RequestVariables);

    private void EndStreaming(DuplexStreamingCallResponse resp) {
        async Task<object> CompleteRead() {
            var responseCompletedTask = this.RespViewModel.ResponseCompletedTask;
            if (responseCompletedTask != null) {
                await responseCompletedTask;
            }

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

    private void OnCallResponseChanged(ViewModelBase obj) {
        var reqVm = (DuplexStreamingReqViewModel)obj;
        var resp = reqVm.CallResponse;
        if (resp == null) {
            return;
        }

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
            if (this.ReqViewModel.ClientStreamTreeEditor.StreamItems.FirstOrDefault() is ResponseStreamNode rs) {
                requestStreamVariables = rs.Variables;
            }
            else {
                requestStreamVariables = this._envVars.RequestStreamVariables;
            }
            
            List<VarDefinition> responseStreamVariables;
            if (this.RespViewModel.ServerStreamTreeEditor.StreamItems.FirstOrDefault() is ResponseStreamNode respStream) {
                responseStreamVariables = respStream.Variables;
            }
            else {
                responseStreamVariables = this._envVars.ResponseStreamVariables;
            }
           
            await GrpcUiUtils.ExportRequest(
                mParams,
                requestVariables,
                responseVariables,
                requestStreamVariables,
                responseStreamVariables,
                reqStream,
                this.MethodInfo,
                this.Io);
        }
    } //await this.ReqViewModel.ExportRequest();

    private AllVariableDefinitions _envVars = new();
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

    private void OnIsBusyChanged(ViewModelBase obj) => this.IsBusy = obj.IsBusy;

    private async Task OnStart() {
        try {
            this.IsBusy = true;
            this.RespViewModel.Init(this._envVars);
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallDuplexStreamingFeature(mi, mParams, Current.EnvFilePath, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();
                if (ok) {
                    this.ReqViewModel.SetupDuplexStream((DuplexStreamingCallResponse)response, this._envVars.RequestStreamVariables);
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

    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
            this.ReqViewModel.EndWriteCommand.Execute(Unit.Default);
            //await this.EndClientStreamingCall(this.ReqViewModel.CallResponse);
        }
    }
}