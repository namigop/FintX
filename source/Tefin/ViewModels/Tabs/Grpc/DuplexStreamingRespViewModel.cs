using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Features;
using Tefin.Grpc.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingRespViewModel : StandardResponseViewModel {
    private bool _canRead;

    public DuplexStreamingRespViewModel(MethodInfo methodInfo) : base(methodInfo) {
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(
                    new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate",
                    new GridLength(1, GridUnitType.Star)),
                    x => x.Items, //
                    x => x.Items.Any(),//
                    x => x.IsExpanded)//
            }
        };
    }

    public bool CanRead {
        get => this._canRead;
        private set => this.RaiseAndSetIfChanged(ref this._canRead , value);
    }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();

    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public async Task SetupDuplexStreamNode(object response) {
        var resp = (DuplexStreamingCallResponse)response;
        var streamNode = (ResponseStreamNode)this.StreamItems[0];
        var readDuplexStream = new ReadDuplexStreamFeature();

        try {
            this.IsBusy = true;
            this.CanRead = true;
            var cs = new CancellationTokenSource();
            await foreach (var d in readDuplexStream.ReadResponseStream(resp, cs.Token))
                Dispatcher.UIThread.Post(() => {
                    streamNode.AddItem(d);
                    if (streamNode.Items.Count == 1)
                        streamNode.IsExpanded = true;
                });
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
        base.Show(ok, response, context);
        var resp = (DuplexStreamingCallResponse)response;
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(resp.CallInfo.ResponseItemType);

        var ddd = Activator.CreateInstance(constructedListType);
        var streamNode = new ResponseStreamNode("Duplex Stream", constructedListType, null, ddd, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
    }
}