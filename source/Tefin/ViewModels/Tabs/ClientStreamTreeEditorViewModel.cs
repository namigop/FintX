using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs;

public class ClientStreamTreeEditorViewModel : ViewModelBase, IListEditorViewModel {
    private object _listInstance;
    private readonly Type _listItemType;
    public ClientStreamTreeEditorViewModel(Type listType) {
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
    public IEnumerable<object> GetListItems() {
        dynamic list = this._listInstance;
        foreach (var i in list)
            yield return i;
    }

    public void Show(object listInstance) {
        /*  Tree Structure is
            - ResponseStreamNode //List
               - DefaultNode //ListItem
         */

        this._listInstance = listInstance;
        var streamNode = new ResponseStreamNode("Client Stream", this.ListType, null, listInstance, null);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
        streamNode.Init();
        //
        // var (ok, reqInstance) = TypeBuilder.getDefault(this._listItemType, true, Core.Utils.none<object>(), 0);
        // if (ok) {
        //     streamNode.AddItem(reqInstance);
        // }
        // else
        //     this.Io.Log.Error($"Unable to create an instance for {this._listItemType}");
    }
}