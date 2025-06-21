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
    //private object?[] _methodParameterInstances;
    private IRequestEditorViewModel _requestEditor;
    private bool _showTreeEditor;
    private List<RequestVariable> _requestVariables = [];

    public UnaryReqViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup clientGroup, bool generateFullTree, List<object?>? methodParameterInstances = null) {
        this._clientGroup = clientGroup;
        this.MethodParameterInstances = methodParameterInstances?.ToArray() ?? [];
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

    public bool IsLoaded {
        get;
        set;
    }

    public MethodInfo MethodInfo { get; }

    public object?[] MethodParameterInstances {
        get;
        set;
    }
    public TreeRequestEditorViewModel TreeEditor => _treeEditor;

    public IRequestEditorViewModel RequestEditor {
        get => this._requestEditor;
        private set => this.RaiseAndSetIfChanged(ref this._requestEditor, value);
    }
    
    public (bool, object?[]) GetMethodParameters() => this.RequestEditor.GetParameters();
    

    public void Init(AllVariableDefinitions allVars) {
        this._requestVariables = allVars.RequestVariables;
        this.MethodParameterInstances = this.IsLoaded ? this.GetMethodParameters().Item2 : this.MethodParameterInstances;
        this._requestEditor.Show(this.MethodParameterInstances, this._requestVariables, this._clientGroup);
        this.IsLoaded = true;
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
            
            this.RequestEditor.Show(parameters, this._requestVariables, this._clientGroup);
        }
    }

    private void ShowAsTree() {
        var (ok, parameters) = this._requestEditor.GetParameters();
        this.RequestEditor = this._treeEditor;
        if (ok) {
            // var reqVars = this._envVariables
            //     .Select(t => new RequestVariable() { Tag = t.Tag, JsonPath = t.JsonPath, TypeName = t.Type })
            //     .ToArray();
            this.RequestEditor.Show(parameters, this._requestVariables, this._clientGroup);
        }
    }
}