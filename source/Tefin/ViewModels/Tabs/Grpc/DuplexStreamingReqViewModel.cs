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

public class DuplexStreamingReqViewModel : UnaryReqViewModel {
    private DuplexStreamingCallResponse? _callResponse;
    private bool _canWrite;

    public DuplexStreamingReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object>? methodParameterInstances = null) : base(methodInfo, generateFullTree,
        methodParameterInstances) {
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };

        this.WriteCommand = this.CreateCommand(this.OnWrite);
        this.EndWriteCommand = this.CreateCommand(this.OnEndWrite);
    }

    public DuplexStreamingCallResponse? CallResponse {
        get => this._callResponse;
        set => this.RaiseAndSetIfChanged(ref this._callResponse, value);
    }

    public ICommand EndWriteCommand { get; }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();

    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public ICommand WriteCommand { get; }
    public bool CanWrite {
        get => this._canWrite;
        private set => this.RaiseAndSetIfChanged(ref this._canWrite, value);
    }

    public void SetupDuplexStream(DuplexStreamingCallResponse response) {
        this.CallResponse = response;
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(response.CallInfo.RequestItemType);
        var stream = Activator.CreateInstance(constructedListType);
        var streamNode = new ResponseStreamNode("Duplex Stream", constructedListType, null, stream, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
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
            if (this.CallResponse == null) {
                this.Io.Log.Warn("Unable to write to the duplex stream");
                return;
            }

            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;
            this.CallResponse = await writer.CompleteWrite(this.CallResponse);
            this.IsBusy = false;
        }
        finally {
            this.CanWrite = false;
            this.IsBusy = false;
        }
    }

    private async Task OnWrite() {
        try {
            if (this.CallResponse == null) {
                this.Io.Log.Warn("Unable to write to the request stream");
                return;
            }
            
            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;

            var node = (TypeBaseNode)this.StreamItems[0].Items[0];
            await writer.Write(this.CallResponse, node.Value);
        }
        finally {
            this.IsBusy = false;
        }
    }
}