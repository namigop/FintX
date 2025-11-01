#region

using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public class ListTreeEditorViewModel : ViewModelBase, IListEditorViewModel {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly bool _isRequest;
    private readonly Type _listItemType;
    private readonly string _name;
    private object _listInstance;

    public ListTreeEditorViewModel(string name, Type listType, ProjectTypes.ClientGroup clientGroup, bool isRequest) {
        this._name = name;
        this._clientGroup = clientGroup;
        this._isRequest = isRequest;
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
        this.StreamTree.RowSelection!.SelectionChanged += this.OnSelectionChanged;
        this._listItemType = TypeHelper.getListItemType(listType).Value;
    }

    public ObservableCollection<IExplorerItem> StreamItems { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> StreamTree { get; }

    public Type ListType {
        get;
    }

    public void RemoveSelectedItem() {
        if (this.StreamItems.Count <= 0) {
            return;
        }

        var streamNode = (StreamNode)this.StreamItems[0];
        var selectedNode = streamNode.FindSelected();
        var listItemNode = selectedNode?.FindParentNode<IExplorerItem>(FindSelected);

        if (listItemNode != null) {
            Dispatcher.UIThread.Post(() => {
                streamNode.RemoveItem(listItemNode);
            });
        }

        bool FindSelected(IExplorerItem item) {
            if (item.Parent != null && item.Parent is StreamNode) {
                return true;
            }

            return false;
        }
    }

    public void AddItem(object instance) =>
        Dispatcher.UIThread.Post(() => {
            if (this.StreamItems.Count == 0) {
                return;
            }

            var streamNode = (StreamNode)this.StreamItems[0];
            streamNode.AddItem(instance);
            if (streamNode.Items.Count == 1) {
                streamNode.IsExpanded = true;
            }
        });

    public void Clear() => this.StreamItems.Clear();

    public (bool, object) GetList() => (true, this._listInstance);

    public IEnumerable<object> GetListItems() {
        if (this.StreamItems.Count > 0 && this.StreamItems[0] is StreamNode rs) {
            rs.TryUpdateTemplatedChildNodes(this.Io);
        }

        dynamic list = this._listInstance;
        foreach (var i in list) {
            yield return i;
        }
    }

    public void Show(object listInstance, List<VarDefinition> streamVariables) {
        /*  Tree Structure is
            - ResponseStreamNode //List
               - DefaultNode //ListItem
         */

        this._listInstance = listInstance;
        var streamNode = new StreamNode(this._name, this.ListType, null, listInstance, null, streamVariables,
            this._clientGroup.Path, this._isRequest);
        this.StreamItems.Clear();
        this.StreamItems.Add(streamNode);
        streamNode.Init();
        streamNode.InitVariableNodes(streamVariables, this._clientGroup.Path, this.Io);
    }

    private void OnSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        foreach (var item in e.DeselectedItems.Where(i => i != null)) {
            item!.IsSelected = false;
        }

        foreach (var item in e.SelectedItems.Where(i => i != null)) {
            item!.IsSelected = true;
        }
    }
}