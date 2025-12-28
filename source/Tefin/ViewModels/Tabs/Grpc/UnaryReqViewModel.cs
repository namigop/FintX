#region

using System.Reflection;
using System.Threading;

using Grpc.Core;

using Microsoft.FSharp.Core;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly JsonRequestEditorViewModel _jsonEditor;

    //private object?[] _methodParameterInstances;
    private IRequestEditorViewModel _requestEditor;
    private List<VarDefinition> _requestVariables = [];
    private bool _showTreeEditor;

    public UnaryReqViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup clientGroup, bool generateFullTree,
        List<object?>? methodParameterInstances = null) {
        this._clientGroup = clientGroup;
        this.MethodParameterInstances = methodParameterInstances?.ToArray() ?? [];
        this._showTreeEditor = true;
        this.SubscribeTo(vm => ((UnaryReqViewModel)vm).IsShowingRequestTreeEditor, this.OnShowTreeEditorChanged);
        this.MethodInfo = methodInfo;
        this._jsonEditor = new JsonRequestEditorViewModel(methodInfo);
        this.TreeEditor = new TreeRequestEditorViewModel(methodInfo);
        this._requestEditor = this.TreeEditor;
    }

    public bool IsLoaded {
        get;
        set;
    }

    public bool IsShowingRequestTreeEditor {
        get => this._showTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._showTreeEditor, value);
    }

    public MethodInfo MethodInfo { get; }

    public object?[] MethodParameterInstances {
        get;
        set;
    }

    public IRequestEditorViewModel RequestEditor {
        get => this._requestEditor;
        private set => this.RaiseAndSetIfChanged(ref this._requestEditor, value);
    }

    public TreeRequestEditorViewModel TreeEditor { get; }

    public (bool, object?[]) GetMethodParameters() {
        var reqParams = this.RequestEditor.GetParameters();

        var expected = this.MethodInfo.GetParameters();
        if (expected.Length == reqParams.Item2.Length) {
            return reqParams;
        }

        //gRPC requests should contain 1. "request" (optional) 2.Headers, 3. deadline, 4. Cancellation token
        var methodParams = reqParams.Item2;
        var metadata = methodParams.FirstOrDefault(t => t?.GetType() == typeof(Metadata));
        if (metadata == null) {
            var (_, m) = TypeBuilder.getDefault(typeof(Metadata), true, FSharpOption<object>.None, 1);
            metadata = m;
        }

        var deadline = methodParams.FirstOrDefault(t => t?.GetType() == typeof(DateTime));
        if (deadline == null) {
            var (_, m) = TypeBuilder.getDefault(typeof(DateTime), true, FSharpOption<object>.None, 1);
            deadline = m;
        }

        var newParams = new List<object>();
        var req = this.MethodInfo.GetParameters().FirstOrDefault(t => t.Name == "request");
        var reqInstance = methodParams.FirstOrDefault(t => t?.GetType() == req?.ParameterType);
        if (reqInstance != null) {
            newParams.Add(reqInstance);
        }

        var token = this.RequestEditor.CtsReq?.Token ?? CancellationToken.None;

        newParams.Add(metadata);
        newParams.Add(deadline);
        newParams.Add(token);
        var parameters = newParams.ToArray();
        this.RequestEditor.Show(parameters, this._requestVariables, this._clientGroup);
        return (true, parameters);
    }


    public void Init(List<VarDefinition> requestVariables) {
        this._requestVariables = requestVariables;
        this.MethodParameterInstances =
            this.IsLoaded ? this.GetMethodParameters().Item2 : this.MethodParameterInstances;
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
        this.RequestEditor = this.TreeEditor;
        if (ok) {
            // var reqVars = this._envVariables
            //     .Select(t => new RequestVariable() { Tag = t.Tag, JsonPath = t.JsonPath, TypeName = t.Type })
            //     .ToArray();
            this.RequestEditor.Show(parameters, this._requestVariables, this._clientGroup);
        }
    }
}