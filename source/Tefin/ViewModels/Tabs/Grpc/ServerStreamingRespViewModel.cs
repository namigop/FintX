using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Tefin.Core.Execution;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ServerStreamingRespViewModel : StandardResponseViewModel {

    public ServerStreamingRespViewModel(MethodInfo methodInfo) : base(methodInfo) {
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };
    }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();

    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public async Task SetupServerStreamNode(object response) {
        var resp = (ServerStreamingCallResponse)response;
        var streamNode = (ResponseStreamNode)this.StreamItems[0];
        var readServerStream = new ReadServerStreamFeature();

        try {
            var cs = new CancellationTokenSource();
            await foreach (var d in readServerStream.ReadResponseStream(resp, cs.Token)) {
                streamNode.AddItem(d);
                if (streamNode.Items.Count == 1)
                    streamNode.IsExpanded = true;
            }
        }
        catch (Exception exc) {
            //exception is already logged by the custom AsyncStreamReader
            this.IsBusy = false;
        }
    }

    public override void Show(bool ok, object response, Context context) {
        base.Show(ok, response, context);
        var resp = (ServerStreamingCallResponse)response;
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(resp.CallInfo.ServerStreamItemType);
        var stream = Activator.CreateInstance(constructedListType);
        var streamNode = new ResponseStreamNode("Server Stream", constructedListType, null, stream, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
    }
}