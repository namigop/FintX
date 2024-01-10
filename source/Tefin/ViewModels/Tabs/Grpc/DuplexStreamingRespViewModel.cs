#region

using System.Reflection;
using System.Threading;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Features;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingRespViewModel : StandardResponseViewModel {
    private readonly Type _listType;
    private readonly Type _responseItemType;
    private readonly ListJsonEditorViewModel _serverStreamJsonEditor;
    private readonly ListTreeEditorViewModel _serverStreamTreeEditor;
    private bool _canRead;
    //private CancellationTokenSource? _cs;
    private bool _isShowingServerStreamTree;
    private IListEditorViewModel _serverStreamEditor;

    public DuplexStreamingRespViewModel(MethodInfo methodInfo) : base(methodInfo) {
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._responseItemType = args[1];
        var listType = typeof(List<>);
        this._listType = listType.MakeGenericType(this._responseItemType);

        this._serverStreamTreeEditor = new ListTreeEditorViewModel("response stream", this._listType);
        this._serverStreamJsonEditor = new ListJsonEditorViewModel("response stream", this._listType);
        this._isShowingServerStreamTree = true;
        this._serverStreamEditor = this._serverStreamTreeEditor;

        this.SubscribeTo(vm => ((ServerStreamingRespViewModel)vm).IsShowingServerStreamTree, this.OnIsShowingServerStreamTreeChanged);
    }



    public bool IsShowingServerStreamTree {
        get => this._isShowingServerStreamTree;
        set => this.RaiseAndSetIfChanged(ref this._isShowingServerStreamTree, value);
    }
    public bool CanRead {
        get => this._canRead;
        private set => this.RaiseAndSetIfChanged(ref this._canRead, value);
    }
    public IListEditorViewModel ServerStreamEditor {
        get => this._serverStreamEditor;
        private set => this.RaiseAndSetIfChanged(ref this._serverStreamEditor, value);
    }

    public async Task SetupDuplexStreamNode(object response) {
        var resp = (DuplexStreamingCallResponse)response;
        var readDuplexStream = new ReadDuplexStreamFeature();

        try {
            this.IsBusy = true;
            this.CanRead = true;
            await foreach (var d in readDuplexStream.ReadResponseStream(resp, CancellationToken.None)) {
                this.ServerStreamEditor.AddItem(d);
            }
        }
        catch (Exception exc) {
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
        this.ServerStreamEditor.Show(stream!);
    }
    private void ShowAsJson() {
        var (ok, list) = this._serverStreamEditor.GetList();
        this.ServerStreamEditor = this._serverStreamJsonEditor;
        if (ok)
            this.ServerStreamEditor.Show(list);
    }

    private void ShowAsTree() {
        var (ok, list) = this._serverStreamEditor.GetList();
        this.ServerStreamEditor = this._serverStreamTreeEditor;
        if (ok)
            this.ServerStreamEditor.Show(list);
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
}