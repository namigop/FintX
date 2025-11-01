#region

using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Utils;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryViewModel : GrpCallTypeViewModelBase {
    private AllVariableDefinitions _envVars = new();
    private UnaryReqViewModel _reqViewModel;
    private bool _showTreeEditor;
    private string _statusText;

    public UnaryViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this._reqViewModel = new UnaryReqViewModel(mi, cg, true);
        this.RespViewModel = new UnaryRespViewModel(mi, cg);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StopCommand = this.CreateCommand(this.OnStop);
        this._statusText = "";
        this._showTreeEditor = true;
        this.ReqViewModel.SubscribeTo(vm => ((UnaryReqViewModel)vm).IsShowingRequestTreeEditor,
            this.OnShowTreeEditorChanged);
        this.ExportRequestCommand = this.CreateCommand(this.OnExportRequest);
        this.ImportRequestCommand = this.CreateCommand(this.OnImportRequest);
    }

    public bool CanStop => this.ReqViewModel.RequestEditor.CtsReq != null;
    public ICommand ExportRequestCommand { get; }
    public ICommand ImportRequestCommand { get; }
    public override bool IsLoaded => this.ReqViewModel.IsLoaded;

    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
            this.ReqViewModel.IsShowingRequestTreeEditor = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }

    public UnaryReqViewModel ReqViewModel {
        get => this._reqViewModel;
        private set => this.RaiseAndSetIfChanged(ref this._reqViewModel, value);
    }

    public UnaryRespViewModel RespViewModel { get; }
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
        if (this.ReqViewModel.TreeEditor.Items[0] is MethodInfoNode methodInfoNode) {
            //var methodInfoNode = (MethodInfoNode)this.ReqViewModel.TreeEditor.Items[0]; 
            var requestVariables = methodInfoNode.Variables;

            List<VarDefinition> responseVariables;
            if (this.RespViewModel.TreeResponseEditor.Items.FirstOrDefault() is ResponseNode respNode) {
                responseVariables = respNode.Variables;
            }
            else {
                responseVariables = this._envVars.ResponseVariables;
            }

            if (ok) {
                var feature = new ExportFeature(this.MethodInfo, mParams, requestVariables, responseVariables, [], []);
                var exportReqJson = feature.Export();
                if (!exportReqJson.IsOk) {
                    this.Io.Log.Error(exportReqJson.ErrorValue);
                }
                else {
                    return exportReqJson.ResultValue;
                }
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

    private async Task OnExportRequest() {
        var reqJson = this.GetRequestContent();
        if (!string.IsNullOrWhiteSpace(reqJson)) {
            var fileName = $"{this.MethodInfo.Name}_req{Ext.requestFileExt}";
            await DialogUtils.SaveFile("Export request", fileName, reqJson, "FintX request",
                $"*{Ext.requestFileExt}");
        }
        //await this.ReqViewModel.ExportRequest();
    }

    private async Task OnImportRequest() {
        var fileExtensions = new[] { $"*{Ext.requestFileExt}" };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            this.ImportRequest(files[0]);
        }
    }

    private void OnShowTreeEditorChanged(ViewModelBase obj) {
        this.ReqViewModel = null!;
        this.ReqViewModel = (UnaryReqViewModel)obj;
        //this.RaisePropertyChanged(nameof(this.ReqViewModel));
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init(this._envVars);
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                this.ReqViewModel.RequestEditor.StartRequest();
                this.RaisePropertyChanged(nameof(this.CanStop));

                var clientConfig = this.Client.Config.Value;

                var feature = new CallUnaryFeature(mi, mParams, Current.EnvFilePath, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();

                this.StatusText = $"Elapsed {printTimeSpan(context.Elapsed.Value)}";
                this.RespViewModel.Show(response);
            }
        }
        finally {
            this.IsBusy = false;
            this.ReqViewModel.RequestEditor.EndRequest();
            this.RaisePropertyChanged(nameof(this.CanStop));
        }
    }

    private void OnStop() {
        if (this.CanStop) {
            this.ReqViewModel.RequestEditor.CtsReq!.Cancel();
        }
    }
}