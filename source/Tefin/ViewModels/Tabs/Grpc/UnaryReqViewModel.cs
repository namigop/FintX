#region

using System.Diagnostics;
using System.Reflection;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Utils;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly JsonRequestEditorViewModel _jsonEditor;
    private readonly TreeRequestEditorViewModel _treeEditor;
    private object?[] _methodParameterInstances;
    private IRequestEditorViewModel _requestEditor;

    //private readonly bool _generateFullTree;
    private bool _showTreeEditor;

    private bool _isLoaded = false;
    private List<RequestVariable> _envVariables = [];

    public UnaryReqViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup clientGroup, bool generateFullTree, List<object?>? methodParameterInstances = null) {
        this._clientGroup = clientGroup;
        this._methodParameterInstances = methodParameterInstances?.ToArray() ?? [];
        this._showTreeEditor = true;
        this.SubscribeTo(vm => ((UnaryReqViewModel)vm).IsShowingRequestTreeEditor, this.OnShowTreeEditorChanged);
        this.MethodInfo = methodInfo;
        this._jsonEditor = new JsonRequestEditorViewModel(methodInfo);
        this._treeEditor = new TreeRequestEditorViewModel(methodInfo);
        this._requestEditor = this._treeEditor;
    }

    public List<RequestVariable> EnvVariables => this._envVariables;
    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
    }

    public bool IsLoaded => this._isLoaded;
    public MethodInfo MethodInfo { get; }
    public TreeRequestEditorViewModel TreeEditor => _treeEditor;

    public IRequestEditorViewModel RequestEditor {
        get => this._requestEditor;
        private set => this.RaiseAndSetIfChanged(ref this._requestEditor, value);
    }

    public virtual async Task ExportRequest() {
        var reqJson = this.GetRequestContent();
        if (!string.IsNullOrWhiteSpace(reqJson)) {
            var fileName = $"{this.MethodInfo.Name}_req{Ext.requestFileExt}";
            await DialogUtils.SaveFile("Export request", fileName, reqJson, "FintX request",
                $"*{Ext.requestFileExt}");
        }
    }

    public (bool, object?[]) GetMethodParameters() => this.RequestEditor.GetParameters();

    public virtual string GetRequestContent() {
        var (ok, mParams) = this.GetMethodParameters();
        var methodInfoNode = (MethodInfoNode)this._treeEditor.Items[0];
        var variables = methodInfoNode.Variables;
        if (ok) {
            var feature = new ExportFeature(this.MethodInfo, mParams, variables);
            var exportReqJson = feature.Export();
            if (!exportReqJson.IsOk) {
                this.Io.Log.Error(exportReqJson.ErrorValue);
            }
            else {
                return exportReqJson.ResultValue;
            }
        }

        return "";
    }

    public virtual async Task ImportRequest() {
        var fileExtensions = new[] { $"*{Ext.requestFileExt}" };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            this.ImportRequestFile(files[0]);
        }
    }

    public virtual void ImportRequestFile(string file) {
        this._isLoaded = false;
        var import = new ImportFeature(this.Io, file, this.MethodInfo);
        var importResult = import.Run();
        if (importResult.IsOk) {
            //1. Show the request
            var methodParams = importResult.ResultValue.MethodParameters;
            if (methodParams == null) {
                Debugger.Break();
            }

            this._methodParameterInstances = methodParams ?? [];

            //these variables, which are stored in the request file, do not contain
            //the current value.  Those are in the *.fxv file in client/var folder
            this._envVariables =
                importResult.ResultValue.Variables
                    .Select(t => new RequestVariable() { Tag = t.Tag, JsonPath = t.JsonPath, TypeName = SystemType.getActualType(t.Type), Scope = t.Scope })
                    .ToList();
            this.Init();
        }
        else {
            this.Io.Log.Error(importResult.ErrorValue);
        }
    }

    public void Init() {
        this._methodParameterInstances = this._isLoaded ? this.GetMethodParameters().Item2 : this._methodParameterInstances;
        this._requestEditor.Show(this._methodParameterInstances, this._envVariables, this._clientGroup);
        this._isLoaded = true;
    }

    private void OnShowTreeEditorChanged(ViewModelBase obj) {
        var vm = (UnaryReqViewModel)obj;
        if (vm.IsShowingRequestTreeEditor) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }

    private void ShowAsJson() {
        var (ok, parameters) = this._requestEditor.GetParameters();
        this.RequestEditor = this._jsonEditor;
        if (ok) {
            // var reqVars = this._envVariables
            //     .Select(t => new RequestVariable() { Tag = t.Tag, JsonPath = t.JsonPath, TypeName = t.TypeName})
            //     .ToArray();
            
            this.RequestEditor.Show(parameters, this._envVariables, this._clientGroup);
        }
    }

    private void ShowAsTree() {
        var (ok, parameters) = this._requestEditor.GetParameters();
        this.RequestEditor = this._treeEditor;
        if (ok) {
            // var reqVars = this._envVariables
            //     .Select(t => new RequestVariable() { Tag = t.Tag, JsonPath = t.JsonPath, TypeName = t.Type })
            //     .ToArray();
            this.RequestEditor.Show(parameters, this._envVariables, this._clientGroup);
        }
    }
}