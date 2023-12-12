#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core;
using Tefin.Grpc;
using Tefin.Grpc.Dynamic;
using Tefin.Utils;
using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private object?[] _methodParameterInstances;

    //private readonly bool _generateFullTree;
    private bool _showTreeEditor;

    private readonly JsonRequestEditorViewModel _jsonEditor;
    private readonly TreeRequestEditorViewModel _treeEditor;
    private IRequestEditorViewModel _requestEditor;

    public UnaryReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object?>? methodParameterInstances = null) {
        this._methodParameterInstances = methodParameterInstances?.ToArray() ?? Array.Empty<object?>();
        this._showTreeEditor = true;
        this.SubscribeTo(vm => ((UnaryReqViewModel)vm).ShowTreeEditor, OnShowTreeEditorChanged);
        this.MethodInfo = methodInfo;
        this._jsonEditor = new JsonRequestEditorViewModel(methodInfo);
        this._treeEditor = new TreeRequestEditorViewModel(methodInfo);
        this._requestEditor = this._treeEditor;
    }

    public IRequestEditorViewModel RequestEditor {
        get => this._requestEditor;
        private set => this.RaiseAndSetIfChanged(ref _requestEditor, value);
    }

    public bool ShowTreeEditor {
        get => this._showTreeEditor;
        set => this.RaiseAndSetIfChanged(ref _showTreeEditor, value);
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
        if (vm.ShowTreeEditor) {
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

    public async Task ImportRequest() {
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "", new[] { Ext.requestFileExt }, false);
        if (ok) {
            var export = Export.importReq(this.Io, new SerParam(this.MethodInfo, Array.Empty<object>(), none<object>()), files[0]);
            if (export.IsOk) {
                var methodParams = export.ResultValue;
                this._methodParameterInstances = methodParams;
                this.Init();
            }
            else {
                Io.Log.Error(export.ErrorValue);
            }
        }
    }
    public async Task ExportRequest() {
        var (ok, mParams) = this.GetMethodParameters();
        if (ok) {
            var sdParam = new SerParam(this.MethodInfo, mParams, none<object>());
            var exportReqJson = Export.requestToJson(sdParam);
            if (exportReqJson.IsOk) {
                var fileName = $"{this.MethodInfo.Name}_req{Ext.requestFileExt}";
                await DialogUtils.SaveFile("Export request", fileName, exportReqJson.ResultValue);
            }
            else {
                Io.Log.Error(exportReqJson.ErrorValue);
            }
        }
    }
}