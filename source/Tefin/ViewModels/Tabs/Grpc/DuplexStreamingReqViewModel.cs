#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingReqViewModel : UnaryReqViewModel {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly ListJsonEditorViewModel _clientStreamJsonEditor;
    private readonly ListTreeEditorViewModel _clientStreamTreeEditor;
    
    private readonly Type _requestItemType;
    private DuplexStreamingCallResponse? _callResponse;
    private bool _canWrite;

    private IListEditorViewModel _clientStreamEditor;
    private bool _isShowingClientStreamTree;
    

    public DuplexStreamingReqViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg, bool generateFullTree, List<object?>? methodParameterInstances = null)
        : base(methodInfo, cg, generateFullTree, methodParameterInstances) {
        this._clientGroup = cg;
        this.WriteCommand = this.CreateCommand(this.OnWrite);
        this.EndWriteCommand = this.CreateCommand(this.OnEndWrite);
        this.AddListItemCommand = this.CreateCommand(this.OnAddListItem);
        this.RemoveListItemCommand = this.CreateCommand(this.OnRemoveListItem);

        
        this._callResponse = DuplexStreamingCallResponse.Empty();
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._requestItemType = args[0];
        var listType = typeof(List<>);
        this.ListType = listType.MakeGenericType(this._requestItemType);

        this._clientStreamTreeEditor = new ListTreeEditorViewModel("RequestStream", this.ListType, cg);
        this._clientStreamJsonEditor = new ListJsonEditorViewModel("RequestStream", this.ListType, cg);
        this._isShowingClientStreamTree = true;
        this._clientStreamEditor = this._clientStreamTreeEditor;

        this.SubscribeTo(vm => ((ClientStreamingReqViewModel)vm).IsShowingClientStreamTree,
            this.OnIsShowingClientStreamTreeChanged);
    }

    public Type ListType { get; }
    public DuplexStreamingCallResponse? CallResponse {
        get => this._callResponse;
        private set => this.RaiseAndSetIfChanged(ref this._callResponse, value);
    }
    public List<VarDefinition> RequestVariables { get; set; }
    public List<VarDefinition> RequestStreamVariables { get; set; }
    public bool CanWrite {
        get => this._canWrite;
        private set => this.RaiseAndSetIfChanged(ref this._canWrite, value);
    }

    public ListTreeEditorViewModel ClientStreamTreeEditor => this._clientStreamTreeEditor;
    public IListEditorViewModel ClientStreamEditor {
        get => this._clientStreamEditor;
        private set => this.RaiseAndSetIfChanged(ref this._clientStreamEditor, value);
    }
    
    

    public ICommand EndWriteCommand { get; }

    public bool IsShowingClientStreamTree {
        get => this._isShowingClientStreamTree;
        set => this.RaiseAndSetIfChanged(ref this._isShowingClientStreamTree, value);
    }

    public ICommand WriteCommand { get; }
    public ICommand RemoveListItemCommand { get; }
    public ICommand AddListItemCommand { get; }

    private void OnAddListItem() {
        var (ok, reqInstance) = TypeBuilder.getDefault(this._requestItemType, true, Core.Utils.none<object>(), 0);
        if (ok) {
            this._clientStreamEditor.AddItem(reqInstance);
        } 
    }
    private void OnRemoveListItem() {
        this._clientStreamEditor.RemoveSelectedItem();
    }
    
    // public async Task ImportRequest() {
    //      await GrpcUiUtils.ImportRequest(this.RequestEditor,
    //          this.ClientStreamEditor, 
    //          this._envVars, 
    //          this.ListType, 
    //          this.MethodInfo,
    //          this._clientGroup,
    //          this.Io);
    // }

    // public override void ImportRequestFile(string file) => GrpcUiUtils.ImportRequest(this.RequestEditor,
    //     this.ClientStreamEditor, this.EnvVariables, this.ListType, this.MethodInfo, this._clientGroup, file, this.Io);

    public void SetupDuplexStream(DuplexStreamingCallResponse response, List<VarDefinition> requestStreamVariables) {
        this._callResponse = response;
        if (this._clientStreamEditor.GetListItems().Any()) {
            this.CanWrite = true;
            return;
        }

        var stream = Activator.CreateInstance(this.ListType)!;
        var (ok, reqInstance) = TypeBuilder.getDefault(this._requestItemType, true, none<object>(), 0);
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

    private async Task OnEndWrite() {
        try {
            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;
            if (this.CallResponse != null) {
                this.CallResponse = await writer.CompleteWrite(this.CallResponse);
            }
        }
        finally {
            this.CanWrite = false;
            //this.ClientStreamEditor.Clear();
            this.IsBusy = false;
        }
    }

    private void OnIsShowingClientStreamTreeChanged(ViewModelBase obj) {
        var vm = (DuplexStreamingReqViewModel)obj;
        if (vm._isShowingClientStreamTree) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }

    private async Task OnWrite() {
        try {
            if (this.CallResponse == null) {
                this.Io.Log.Warn("Unable to write to the request stream");
                return;
            }

            var resp = this.CallResponse;
            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;

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

    private void ShowAsJson() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this._clientStreamJsonEditor;
        if (ok) {
            this.ClientStreamEditor.Show(list, this.RequestStreamVariables);
        }
    }

    private void ShowAsTree() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this._clientStreamTreeEditor;
        if (ok) {
            this.ClientStreamEditor.Show(list, this.RequestStreamVariables);
        }
    }
}