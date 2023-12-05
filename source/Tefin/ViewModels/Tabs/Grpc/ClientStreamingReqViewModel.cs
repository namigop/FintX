using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ClientStreamingReqViewModel : UnaryReqViewModel {
    private ClientStreamingCallResponse? _callResponse;
    private bool _canWrite;

    public ClientStreamingReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object>? methodParameterInstances = null) : base(methodInfo, generateFullTree,
        methodParameterInstances) {
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };

        this.WriteCommand = this.CreateCommand(this.OnWrite);
        this.EndWriteCommand = this.CreateCommand(this.OnEndWrite);
        _callResponse = default;
        this.StreamItems.Add(new EmptyNode());
    }

    public ClientStreamingCallResponse? CallResponse {
        get => this._callResponse;
        set => this.RaiseAndSetIfChanged(ref this._callResponse, value);
    }

    public ICommand EndWriteCommand { get; }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();

    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public ICommand WriteCommand { get; }


    public bool CanWrite {
        get => _canWrite;
        set => this.RaiseAndSetIfChanged(ref _canWrite, value);
    }

    public void SetupClientStream(ClientStreamingCallResponse response) {
        this._callResponse = response;
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(response.CallInfo.RequestItemType);
        var stream = Activator.CreateInstance(constructedListType);
        var streamNode = new ResponseStreamNode("Client Stream", constructedListType, null, stream, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);

        streamNode.Items.CollectionChanged += (ss, args) => {
        };
        var (ok, reqInstance) = TypeBuilder.getDefault(response.CallInfo.RequestItemType, true, none<object>(), 0);
        if (ok) {
            this.CanWrite = true;
            streamNode.AddItem(reqInstance);
        }
        else
            this.Io.Log.Error($"Unable to create an instance for {response.CallInfo.RequestItemType}");
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
            this.IsBusy = false;
        }
    }

    private async Task OnWrite() {
        try {
            var resp = this.CallResponse;
            this.IsBusy = true;
            var writer = new WriteClientStreamFeature();
            var node = (TypeBaseNode)this.StreamItems[0].Items[0];
            await writer.Write(resp, node.Value);
        }
        finally {
            this.IsBusy = false;
        }
    }
}