#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingReqViewModel : UnaryReqViewModel {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly ListJsonEditorViewModel _clientStreamJsonEditor;
    private readonly Type _requestItemType;
    private ClientStreamingCallResponse _callResponse;
    private bool _canWrite;

    private IListEditorViewModel _clientStreamEditor;

    private bool _isShowingClientStreamTree;
    //private AllVariableDefinitions? _envVars;

    public ClientStreamingReqViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg, bool generateFullTree,
        List<object?>? methodParameterInstances = null)
        : base(methodInfo, cg, generateFullTree, methodParameterInstances) {
        this._clientGroup = cg;
        this.WriteCommand = this.CreateCommand(this.OnWrite);
        this.EndWriteCommand = this.CreateCommand(this.OnEndWrite);
        this.AddListItemCommand = this.CreateCommand(this.OnAddListItem);
        this.RemoveListItemCommand = this.CreateCommand(this.OnRemoveListItem);
        this._callResponse = ClientStreamingCallResponse.Empty();
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._requestItemType = args[0];
        var listType = typeof(List<>);
        this.ListType = listType.MakeGenericType(this._requestItemType);
        this.ClientStreamTreeEditor = new ListTreeEditorViewModel("ClientStream", this.ListType, cg, true);
        this._clientStreamJsonEditor = new ListJsonEditorViewModel("ClientStream", this.ListType, cg, true);
        this._isShowingClientStreamTree = true;
        this._clientStreamEditor = this.ClientStreamTreeEditor;

        this.SubscribeTo(vm => ((ClientStreamingReqViewModel)vm).IsShowingClientStreamTree,
            this.OnIsShowingClientStreamTreeChanged);
    }

    public ICommand AddListItemCommand { get; }

    public ClientStreamingCallResponse CallResponse {
        get => this._callResponse;
        private set => this.RaiseAndSetIfChanged(ref this._callResponse, value);
    }

    public bool CanWrite {
        get => this._canWrite;
        set => this.RaiseAndSetIfChanged(ref this._canWrite, value);
    }

    public IListEditorViewModel ClientStreamEditor {
        get => this._clientStreamEditor;
        private set => this.RaiseAndSetIfChanged(ref this._clientStreamEditor, value);
    }

    public ListTreeEditorViewModel ClientStreamTreeEditor { get; }

    public ICommand EndWriteCommand {
        get;
    }

    public bool IsShowingClientStreamTree {
        get => this._isShowingClientStreamTree;
        set => this.RaiseAndSetIfChanged(ref this._isShowingClientStreamTree, value);
    }

    public Type ListType { get; }
    public ICommand RemoveListItemCommand { get; }
    public List<VarDefinition> RequestStreamVariables { get; set; }

    public List<VarDefinition> RequestVariables { get; set; }

    public ICommand WriteCommand {
        get;
    }

    private void OnAddListItem() {
        var (ok, reqInstance) = TypeBuilder.getDefault(this._requestItemType, true, Core.Utils.none<object>(), 0);
        if (ok) {
            this._clientStreamEditor.AddItem(reqInstance);
        }
    }

    private async Task OnEndWrite() {
        try {
            var resp = this.CallResponse;
            this.IsBusy = true;
            var writer = new WriteClientStreamFeature();
            this.CallResponse = await writer.CompleteWrite(resp);
        }
        finally {
            this.CanWrite = false;
            //this._clientStreamEditor.Clear();
            this.IsBusy = false;
        }
    }

    private void OnIsShowingClientStreamTreeChanged(ViewModelBase obj) {
        var vm = (ClientStreamingReqViewModel)obj;
        if (vm._isShowingClientStreamTree) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }

    private void OnRemoveListItem() => this._clientStreamEditor.RemoveSelectedItem();

    private async Task OnWrite() {
        try {
            var resp = this.CallResponse;
            this.IsBusy = true;
            var writer = new WriteClientStreamFeature();

            foreach (var i in this.ClientStreamEditor.GetListItems()) {
                await writer.Write(resp, i);
            }
        }
        catch (Exception exc) {
            this.Io.Log.Error(exc);
        }
        finally {
            this.IsBusy = false;
        }
    }

    // public override async Task ImportRequest() => await GrpcUiUtils.ImportRequest(
    //     this.RequestEditor,
    //     this.ClientStreamEditor,
    //     this.EnvVariables,
    //     this.ListType,
    //     this.MethodInfo, 
    //     this._clientGroup, 
    //     this.Io);

    // public override void ImportRequestFile(string file) {
    //     GrpcUiUtils.ImportRequest(
    //         this.RequestEditor,
    //         this.ClientStreamEditor,
    //         this.EnvVariables,
    //         this.ListType,
    //         this.MethodInfo,
    //         this._clientGroup,
    //         file,
    //         this.Io);
    //     
    //     //load variables
    //     //these variables, which are stored in the request file, do not contain
    //     //the current value.  Those are in the *.fxv file in client/var folder
    //     
    //     this.IsLoaded = true;
    // }

    public void SetupClientStream(ClientStreamingCallResponse response, List<VarDefinition> requestStreamVariables) {
        this._callResponse = response;
        if (this._clientStreamEditor.GetListItems().Any()) {
            this.CanWrite = true;
            return;
        }

        var stream = Activator.CreateInstance(this.ListType)!;
        var (ok, reqInstance) = TypeBuilder.getDefault(this._requestItemType, true, Core.Utils.none<object>(), 0);
        if (ok) {
            var add = this.ListType.GetMethod("Add");
            add!.Invoke(stream, [reqInstance]);
        }
        else {
            this.Io.Log.Error($"Unable to create an instance for {this._requestItemType}");
        }

        this.RequestStreamVariables = requestStreamVariables;
        this._clientStreamEditor.Show(stream!, this.RequestStreamVariables);
        this.CanWrite = true;
    }

    private void ShowAsJson() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this._clientStreamJsonEditor;
        if (ok) {
            this.ClientStreamEditor.Show(list, this.RequestStreamVariables);
        }
    }

    private void ShowAsTree() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this.ClientStreamTreeEditor;
        if (ok) {
            this.ClientStreamEditor.Show(list, this.RequestStreamVariables);
        }
    }
}