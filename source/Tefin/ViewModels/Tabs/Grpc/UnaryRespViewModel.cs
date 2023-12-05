using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;

using Tefin.Core.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryRespViewModel : ViewModelBase {
    private readonly MethodInfo _methodInfo;

    public UnaryRespViewModel(MethodInfo methodInfo) {
        this._methodInfo = methodInfo;
        this.ResponseTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };
    }

    public ObservableCollection<IExplorerItem> Items { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> ResponseTree { get; }

    public void Init() {
        this.Items.Clear();
    }

    public void Show(bool ok, object response, Context context) {
        Dispatcher.UIThread.Post(() => {
            this.Items.Clear();
            var node = new ResponseNode(this._methodInfo.Name, response.GetType(), null, response, null); // TypeNodeBuilder.Create(this._methodInfo.Name, response);
            node.Init();
            this.Items.Add(node);
        });
    }
}