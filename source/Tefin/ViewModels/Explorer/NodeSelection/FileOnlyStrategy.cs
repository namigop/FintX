using Avalonia.Controls;
using Avalonia.Controls.Selection;

using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.ServiceMock;

namespace Tefin.ViewModels.Explorer;

/// <summary>
///     Single selection for non-FileNodes.  MultipleSelection for FileNodes
/// </summary>
public class FileOnlyStrategy : IExplorerNodeSelectionStrategy {
    private readonly Func<IExplorerItem, NodeBase?> _findParentNode;

    //private readonly ClientExplorerViewModel _explorerViewModel;
    private readonly Func<IExplorerItem?> _getSelected;

    /// <summary>
    ///     Single selection for non-FileNodes.  MultipleSelection for FileNodes
    /// </summary>
    /// <param name="explorerViewModel"></param>
    public FileOnlyStrategy(ClientExplorerViewModel explorerViewModel) {
        //this._explorerViewModel = explorerViewModel;
        this.ExplorerTree = explorerViewModel.ExplorerTree;
        var getSelected =
            () => explorerViewModel.GetClientNodes()
                .Select(c => c.FindSelected())
                .FirstOrDefault(m => m != null);

        this._getSelected = getSelected;
        this._findParentNode = i => i.FindParentNode<ClientRootNode>();
    }

    /// <summary>
    ///     Single selection for non-FileNodes.  MultipleSelection for FileNodes
    /// </summary>
    /// <param name="explorerViewModel"></param>
    public FileOnlyStrategy(ServiceMockExplorerViewModel explorerViewModel) {
        this.ExplorerTree = explorerViewModel.ExplorerTree;
        var getSelected =
            () => explorerViewModel.GetServiceMockNodes()
                .Select(c => c.FindSelected())
                .FirstOrDefault(m => m != null);

        this._getSelected = getSelected;
        this._findParentNode = i => i.FindParentNode<ServiceMockRootNode>();
    }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; }

    public void Apply(TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        var selected = this._getSelected();

        List<IExplorerItem?> selectedItems = [];
        for (var i = 0; i < e.SelectedItems.Count; i++) {
            selectedItems.Add(e.SelectedItems[i]!);
        }

        List<IndexPath> selectedIndexes = [];
        for (var i = 0; i < e.SelectedIndexes.Count; i++) {
            selectedIndexes.Add(e.SelectedIndexes[i]);
        }

        var index = -1;
        foreach (var item in selectedItems) {
            index += 1;

            if (item == null) {
                continue;
            }

            //if there was no previous selection
            if (selected == null) {
                item.IsSelected = true;
                selected = item;
                continue;
            }

            if (selected is FileNode && item is FileNode fn) {
                var p1 = this._findParentNode(selected); //FindParentNode<ClientRootNode>();
                var p2 = this._findParentNode(item); //.FindParentNode<ClientRootNode>();

                //allow selection only if the nodes have the same parent client node 
                if (p1 == p2) {
                    fn.IsSelected = true;
                    selected = fn;
                    continue;
                }
            }

            var d = selectedIndexes[index];
            this.ExplorerTree.RowSelection!.Deselect(d);
        }
    }
}