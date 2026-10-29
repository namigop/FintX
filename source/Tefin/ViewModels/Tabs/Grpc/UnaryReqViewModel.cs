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
        var (ok, reqParams) = this.RequestEditor.GetParameters();
        if (!ok) {
            //the request could not be built. Substituting defaults here would destroy the
            //request that the user typed and run the call with the wrong arity, so the
            //failure is reported to the caller instead.
            return (false, reqParams);
        }

        var expected = this.MethodInfo.GetParameters();
        if (expected.Length == reqParams.Length) {
            return (true, reqParams);
        }

        //gRPC requests should contain 1. "request" (optional) 2.Headers, 3. deadline, 4. Cancellation token
        var methodParams = reqParams;
        var metadata = methodParams.FirstOrDefault(t => t?.GetType() == typeof(Metadata));
        if (metadata == null) {
            var (_, m) = TypeBuilder.getDefault("headers", typeof(Metadata), true, FSharpOption<object>.None, 1);
            metadata = m;
        }

        var deadline = methodParams.FirstOrDefault(t => t?.GetType() == typeof(DateTime));
        if (deadline == null) {
            var (_, m) = TypeBuilder.getDefault("deadline", typeof(DateTime), true, FSharpOption<object>.None, 1);
            deadline = m;
        }

        var token = this.RequestEditor.CtsReq?.Token ?? CancellationToken.None;

        //build the argument array positionally against the method signature, so that a
        //request that cannot be found can never shorten it
        var parameters = expected.Select(p => {
            if (p.ParameterType == typeof(Metadata)) {
                return metadata;
            }

            if (p.ParameterType == typeof(DateTime) || p.ParameterType == typeof(DateTime?)) {
                return deadline;
            }

            if (p.ParameterType == typeof(CancellationToken)) {
                return token;
            }

            var instance = methodParams.FirstOrDefault(t => t?.GetType() == p.ParameterType);
            return instance ?? TypeBuilder.getDefault(p.Name, p.ParameterType, true, FSharpOption<object>.None, 1).Item2;
        }).ToArray();

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