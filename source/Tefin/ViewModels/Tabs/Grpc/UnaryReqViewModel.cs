#region

using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

using ReactiveUI;

using Tefin.Core;
using Tefin.Features;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private readonly JsonRequestEditorViewModel _jsonEditor;
    private readonly TreeRequestEditorViewModel _treeEditor;
    private object?[] _methodParameterInstances;
    private IRequestEditorViewModel _requestEditor;

    //private readonly bool _generateFullTree;
    private bool _showTreeEditor;

    private bool _isLoaded = false;

    public UnaryReqViewModel(MethodInfo methodInfo, bool generateFullTree,
        List<object?>? methodParameterInstances = null) {
        this._methodParameterInstances = methodParameterInstances?.ToArray() ?? [];
        this._showTreeEditor = true;
        this.SubscribeTo(vm => ((UnaryReqViewModel)vm).IsShowingRequestTreeEditor, this.OnShowTreeEditorChanged);
        this.MethodInfo = methodInfo;
        this._jsonEditor = new JsonRequestEditorViewModel(methodInfo);
        this._treeEditor = new TreeRequestEditorViewModel(methodInfo);
        this._requestEditor = this._treeEditor;
    }

    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
    }

    public MethodInfo MethodInfo { get; }

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
        if (ok) {
            var feature = new ExportFeature(this.MethodInfo, mParams);
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
        var import = new ImportFeature(this.Io, file, this.MethodInfo);
        var (export, _) = import.Run();
        if (export.IsOk) {
            var methodParams = export.ResultValue;
            if (methodParams == null) {
                Debugger.Break();
            }

            this._methodParameterInstances = methodParams ?? [];
            this.Init();
        }
        else {
            this.Io.Log.Error(export.ErrorValue);
        }
    }

    public void Init() {
        this._methodParameterInstances =
            this._isLoaded ? this.GetMethodParameters().Item2 : this._methodParameterInstances;
        this._requestEditor.Show(this._methodParameterInstances);
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
            this.RequestEditor.Show(parameters);
        }
    }

    private void ShowAsTree() {
        var (ok, parameters) = this._requestEditor.GetParameters();
        this.RequestEditor = this._treeEditor;
        if (ok) {
            this.RequestEditor.Show(parameters);
        }
    }
}