using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Threading;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs;

public class ListTreeEditorViewModel : ViewModelBase, IListEditorViewModel {
    private readonly string _name;
    private object _listInstance;
    private readonly Type _listItemType;
    public ListTreeEditorViewModel(string name, Type listType) {
        this._name = name;
        this.ListType = listType;
        this._listInstance = Activator.CreateInstance(listType)!;
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };

        this._listItemType = TypeHelper.getListItemType(listType).Value;
    }
    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public Type ListType {
        get;
    }

    public (bool, object) GetList() {
        return (true, this._listInstance);
    }

    public void AddItem(object instance) {
        Dispatcher.UIThread.Post(() => {
            var streamNode = (ResponseStreamNode)this.StreamItems[0];
            streamNode.AddItem(instance);
            if (streamNode.Items.Count == 1)
                streamNode.IsExpanded = true;
        });
    }
    public IEnumerable<object> GetListItems() {
        dynamic list = this._listInstance;
        foreach (var i in list)
            yield return i;
    }

    public void Clear() {
        this.StreamItems.Clear();
    }

    public void Show(object listInstance) {
        /*  Tree Structure is
            - ResponseStreamNode //List
               - DefaultNode //ListItem
         */

        this._listInstance = listInstance;
        var streamNode = new ResponseStreamNode(this._name, this.ListType, null, listInstance, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
        streamNode.Init();
    }
}