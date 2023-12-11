using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingRespViewModel : StandardResponseViewModel {
    private bool _canRead;
    private readonly Type _responseItemType;
    private readonly Type _listType;
    private readonly ListTreeEditorViewModel _serverStreamTreeEditor;
    private readonly ListJsonEditorViewModel _serverStreamJsonEditor;
    private bool _isShowingServerStreamTree;
    private IListEditorViewModel _serverStreamEditor;

    public ServerStreamingRespViewModel(MethodInfo methodInfo) : base(methodInfo) {
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._responseItemType = args[0];
        var listType = typeof(List<>);
        this._listType = listType.MakeGenericType(_responseItemType);

        this._serverStreamTreeEditor = new ListTreeEditorViewModel("Server Stream", this._listType);
        this._serverStreamJsonEditor = new ListJsonEditorViewModel("Server Stream", this._listType);
        this._isShowingServerStreamTree = true;
        this._serverStreamEditor = this._serverStreamTreeEditor;
        this.EndReadCommand = CreateCommand(OnEndRead);

        this.SubscribeTo(vm => ((ServerStreamingRespViewModel)vm).IsShowingServerStreamTree, OnIsShowingServerStreamTreeChanged);
    }

    public ICommand EndReadCommand {
        get;
    }

    private Task OnEndRead() {
        throw new NotImplementedException();
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
    public async Task SetupServerStreamNode(object response) {
        var resp = (ServerStreamingCallResponse)response;

        var readServerStream = new ReadServerStreamFeature();

        try {
            var cs = new CancellationTokenSource();
            this.CanRead = true;
            await foreach (var d in readServerStream.ReadResponseStream(resp, cs.Token)) {
                this.ServerStreamEditor.AddItem(d);
            }
        }
        finally {
            this.IsBusy = false;
            this.CanRead = false;
        }
    }

    public override void Show(bool ok, object response, Context context) {
        base.Show(ok, response, context);
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
        var vm = (ServerStreamingRespViewModel)obj;
        if (vm._isShowingServerStreamTree) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }
}