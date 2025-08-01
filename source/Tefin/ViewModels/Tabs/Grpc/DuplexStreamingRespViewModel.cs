#region

using System.Reflection;
using System.Threading;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingRespViewModel : StandardResponseViewModel {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly Type _listType;
    private readonly Type _responseItemType;
    private readonly ListJsonEditorViewModel _serverStreamJsonEditor;
    private readonly ListTreeEditorViewModel _serverStreamTreeEditor;
    private bool _canRead;

    //private CancellationTokenSource? _cs;
    private bool _isShowingServerStreamTree;

    private IListEditorViewModel _serverStreamEditor;

    public DuplexStreamingRespViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) : base(methodInfo, cg) {
        this._clientGroup = cg;
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._responseItemType = args[1];
        var listType = typeof(List<>);
        this._listType = listType.MakeGenericType(this._responseItemType);

        this._serverStreamTreeEditor = new ListTreeEditorViewModel("ResponseStream", this._listType, cg, false);
        this._serverStreamJsonEditor = new ListJsonEditorViewModel("ResponseStream", this._listType, cg, false);
        this._isShowingServerStreamTree = true;
        this._serverStreamEditor = this._serverStreamTreeEditor;

        this.SubscribeTo(vm => ((ServerStreamingRespViewModel)vm).IsShowingServerStreamTree,
            this.OnIsShowingServerStreamTreeChanged);
    }

    public bool CanRead {
        get => this._canRead;
        private set => this.RaiseAndSetIfChanged(ref this._canRead, value);
    }

    public bool IsShowingServerStreamTree {
        get => this._isShowingServerStreamTree;
        set => this.RaiseAndSetIfChanged(ref this._isShowingServerStreamTree, value);
    }

    public ListTreeEditorViewModel ServerStreamTreeEditor => _serverStreamTreeEditor;

    public IListEditorViewModel ServerStreamEditor {
        get => this._serverStreamEditor;
        private set => this.RaiseAndSetIfChanged(ref this._serverStreamEditor, value);
    }

    public List<VarDefinition> ResponseStreamVariables { get; private set; } = [];
    public Task? ResponseCompletedTask { get; private set; }

    public override void Init(AllVariableDefinitions envVariables) {
        this.ResponseVariables = envVariables.ResponseVariables;
        this.ResponseStreamVariables = envVariables.ResponseStreamVariables;
        this.ResponseEditor.Init();
    }
    
    public async Task SetupDuplexStreamNode(object response) {
        var resp = (DuplexStreamingCallResponse)response;
        var readDuplexStream = new ReadDuplexStreamFeature();
        var tcs = new TaskCompletionSource();

        this.ResponseCompletedTask = tcs.Task;
        try {
            this.IsBusy = true;
            this.CanRead = true;

            await foreach (var d in readDuplexStream.ReadResponseStream(resp, CancellationToken.None)) {
                this.ServerStreamEditor.AddItem(d);
            }

            tcs.SetResult();
        }
        catch (Exception exc) {
            tcs.SetException(exc);
            this.Io.Log.Error(exc);
        }
        finally {
            this.IsBusy = false;
            this.CanRead = false;
        }
    }

    public override void Show(bool ok, object response, Context context) {
        //base.Show(ok, response, context);
        var stream = Activator.CreateInstance(this._listType);
        this.ServerStreamEditor.Show(stream!, this.ResponseStreamVariables);
    }

    private void OnIsShowingServerStreamTreeChanged(ViewModelBase obj) {
        var vm = (DuplexStreamingRespViewModel)obj;
        if (vm._isShowingServerStreamTree) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }

    private void ShowAsJson() {
        var (ok, list) = this._serverStreamEditor.GetList();
        this.ServerStreamEditor = this._serverStreamJsonEditor;
        if (ok) {
            this.ServerStreamEditor.Show(list, this.ResponseStreamVariables);
        }
    }

    private void ShowAsTree() {
        var (ok, list) = this._serverStreamEditor.GetList();
        this.ServerStreamEditor = this._serverStreamTreeEditor;
        if (ok) {
            this.ServerStreamEditor.Show(list, this.ResponseStreamVariables);
        }
    }
}