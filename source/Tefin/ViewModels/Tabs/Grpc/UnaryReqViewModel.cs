#region

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

    public UnaryReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object?>? methodParameterInstances = null) {
        this._methodParameterInstances = methodParameterInstances?.ToArray() ?? Array.Empty<object?>();
        this._showTreeEditor = true;
        this.SubscribeTo(vm => ((UnaryReqViewModel)vm).IsShowingRequestTreeEditor, this.OnShowTreeEditorChanged);
        this.MethodInfo = methodInfo;
        this._jsonEditor = new JsonRequestEditorViewModel(methodInfo);
        this._treeEditor = new TreeRequestEditorViewModel(methodInfo);
        this._requestEditor = this._treeEditor;
    }

    public IRequestEditorViewModel RequestEditor {
        get => this._requestEditor;
        private set => this.RaiseAndSetIfChanged(ref this._requestEditor, value);
    }

    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
    }

    public MethodInfo MethodInfo { get; }


    public (bool, object?[]) GetMethodParameters() {
        return this.RequestEditor.GetParameters();
    }

    public void Init() {
        this._requestEditor.Show(this._methodParameterInstances);
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
        if (ok)
            this.RequestEditor.Show(parameters);
    }

    private void ShowAsTree() {
        var (ok, parameters) = this._requestEditor.GetParameters();
        this.RequestEditor = this._treeEditor;
        if (ok)
            this.RequestEditor.Show(parameters);
    }

    public virtual async Task ImportRequest() {
        var fileExtensions = new[] {
            $"*{Ext.requestFileExt}"
        };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            var import = new ImportFeature(this.Io, files[0], this.MethodInfo);
            var (export, _) = import.Run();
            if (export.IsOk) {
                var methodParams = export.ResultValue;
                this._methodParameterInstances = methodParams;
                this.Init();
            }
            else {
                this.Io.Log.Error(export.ErrorValue);
            }
        }
    }

    public string GetRequestContent() {
        var (ok, mParams) = this.GetMethodParameters();
        if (ok) {
            var feature = new ExportFeature(this.MethodInfo, mParams);
            var exportReqJson = feature.Export();
            if (exportReqJson.IsOk) {
                return exportReqJson.ResultValue;
            }
        }

        return "";
    }

    
    public virtual async Task ExportRequest() {
        var (ok, mParams) = this.GetMethodParameters();
        if (ok) {
            var feature = new ExportFeature(this.MethodInfo, mParams);
            var exportReqJson = feature.Export();
            if (exportReqJson.IsOk) {
                var fileName = $"{this.MethodInfo.Name}_req{Ext.requestFileExt}";
                await DialogUtils.SaveFile("Export request", fileName, exportReqJson.ResultValue, "FintX request", $"*{Ext.requestFileExt}");
            }
            else {
                this.Io.Log.Error(exportReqJson.ErrorValue);
            }
        }
    }
}