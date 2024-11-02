#region

using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public class ListTreeEditorViewModel : ViewModelBase, IListEditorViewModel {
    private readonly Type _listItemType;
    private readonly string _name;
    private object _listInstance;

    public ListTreeEditorViewModel(string name, Type listType) {
        this._name = name;
        this.ListType = listType;
        this._listInstance = Activator.CreateInstance(listType)!;
        this.StreamTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.StreamItems) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate",
                    "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };

        this.StreamTree.RowSelection!.SingleSelect = true;
        this.StreamTree.RowSelection!.SelectionChanged += OnSelectionChanged;
        this._listItemType = TypeHelper.getListItemType(listType).Value;
    }

    private void OnSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        foreach (var item in e.DeselectedItems.Where(i => i != null)) {
            item!.IsSelected = false;
        }
        foreach (var item in e.SelectedItems.Where(i => i != null)) {
            item!.IsSelected = true;
        }
    }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public Type ListType {
        get;
    }
    public void RemoveSelectedItem() {
        if (this.StreamItems.Count <= 0)
            return;
        
        var streamNode = (ResponseStreamNode)this.StreamItems[0];
        var selectedNode = streamNode.FindSelected();
        var listItemNode = selectedNode?.FindParentNode<IExplorerItem>(FindSelected);

        if (listItemNode != null) {
            Dispatcher.UIThread.Post(() => {
                streamNode.RemoveItem(listItemNode);
            });
        }

        bool FindSelected(IExplorerItem item) {
            if (item.Parent != null && item.Parent is ResponseStreamNode) {
                return true;
            }

            return false;
        }
    }
    public void AddItem(object instance) =>
        Dispatcher.UIThread.Post(() => {
            var streamNode = (ResponseStreamNode)this.StreamItems[0];
            streamNode.AddItem(instance);
            if (streamNode.Items.Count == 1) {
                streamNode.IsExpanded = true;
            }
        });

    public void Clear() => this.StreamItems.Clear();

    public (bool, object) GetList() => (true, this._listInstance);

    public IEnumerable<object> GetListItems() {
        dynamic list = this._listInstance;
        foreach (var i in list) {
            yield return i;
        }
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