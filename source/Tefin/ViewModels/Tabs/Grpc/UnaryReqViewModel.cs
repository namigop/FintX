#region

using System.Reflection;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private readonly object?[] _methodParameterInstances;

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
}