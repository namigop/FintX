#region

using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.Utils;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingViewModel : GrpCallTypeViewModelBase {
    private bool _showTreeEditor;
    private string _statusText;
    private AllVariableDefinitions _envVars = new();

    public ServerStreamingViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new ServerStreamingReqViewModel(mi, cg, true);
        this.RespViewModel = new ServerStreamingRespViewModel(mi, cg);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this._statusText = "";
        this._showTreeEditor = true;
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
        this.RespViewModel.SubscribeTo(x => ((ServerStreamingRespViewModel)x).CanRead, this.OnCanReadChanged);
    }

    public bool CanStop => this.RespViewModel.CanRead && this.ReqViewModel.RequestEditor.CtsReq != null;
    public override bool IsLoaded => this.ReqViewModel.IsLoaded;
    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }

    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
            this.RespViewModel.IsShowingServerStreamTree = value;
        }
    }

    public ServerStreamingReqViewModel ReqViewModel { get; set; }
    public ServerStreamingRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public ICommand StopCommand { get; }

    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override string GetRequestContent() {
        var (ok, mParams) = this.ReqViewModel.GetMethodParameters();
        if (ok) {
            var methodInfoNode = (MethodInfoNode)this.ReqViewModel.TreeEditor.Items[0];
            var requestVariables = methodInfoNode.Variables;

            List<RequestVariable> responseVariables;
            if (this.RespViewModel.TreeResponseEditor.Items.FirstOrDefault() is ResponseNode respNode) {
                responseVariables = respNode.Variables;
            }
            else {
                responseVariables = this._envVars.ResponseVariables;
            }

            List<RequestVariable> responseStreamVariables;
            if (this.RespViewModel.ServerStreamTreeEditor.StreamItems.FirstOrDefault() is ResponseStreamNode respStream) {
                responseStreamVariables = respStream.Variables;
            }
            else {
                responseStreamVariables = this._envVars.ResponseStreamVariables;
            }

            var feature = new ExportFeature(this.MethodInfo, mParams, requestVariables, responseVariables, [], responseStreamVariables);
            var exportReqJson = feature.Export();
            if (exportReqJson.IsOk) {
                return exportReqJson.ResultValue;
            }

        }

        return string.Empty;
    }

    public override void ImportRequest(string requestFile) {
        this.ReqViewModel.IsLoaded = false;
        var import = new ImportFeature(this.Io, requestFile, this.MethodInfo);
        var importResult = import.Run();
        if (importResult.IsOk) {
            //1. Show the request
            var methodParams = importResult.ResultValue.MethodParameters;
            if (methodParams == null) {
                Debugger.Break();
            }

            this.ReqViewModel.MethodParameterInstances = methodParams ?? [];

            //these variables, which are stored in the request file, do not contain
            //the current value.  Those are in the *.fxv file in client/var folder
            this._envVars = AllVariableDefinitions.From(importResult.ResultValue.Variables);
            this.ReqViewModel.Init(this._envVars.RequestVariables);
        }
        else {
            this.Io.Log.Error(importResult.ErrorValue);
        }
    }

    public override void Init() => this.ReqViewModel.Init(this._envVars.RequestVariables);

    private void OnCanReadChanged(ViewModelBase obj) => this.RaisePropertyChanged(nameof(this.CanStop));

    private async Task OnExportRequest() {
        var reqJson = this.GetRequestContent();
        if (!string.IsNullOrWhiteSpace(reqJson)) {
            var fileName = $"{this.MethodInfo.Name}_req{Ext.requestFileExt}";
            await DialogUtils.SaveFile("Export request", fileName, reqJson, "FintX request",
                $"*{Ext.requestFileExt}");
        }
    }

    private async Task OnImportRequest() {
        var fileExtensions = new[] { $"*{Ext.requestFileExt}" };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            this.ImportRequest(files[0]);
        }
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init(this._envVars);
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                this.ReqViewModel.RequestEditor.StartRequest();

                var clientConfig = this.Client.Config.Value;
                var feature = new CallServerStreamingFeature(mi, mParams, Current.EnvFilePath, clientConfig, this.Io);
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

    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
        }
    }
}