using Avalonia.Controls.Selection;

namespace Tefin.ViewModels.Explorer;

/// <summary>
///     Multiple selection allowed only for the types of nodes
/// </summary>
/// <param name="explorerViewModel"></param>
public class SameNodeTypeStrategy<T>(IExplorerTree<T> explorerViewModel) : IExplorerNodeSelectionStrategy where T : NodeBase {
    public void Apply(TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        var selected = explorerViewModel.GetRootNodes().Select(c => c.FindSelected()).FirstOrDefault(m => m != null);
        var nodeType = selected?.GetType();
        var index = -1;
        foreach (var item in e.SelectedItems) {
            index += 1;
            if (item == null) {
                continue;
            }

            if (nodeType == null) {
                item!.IsSelected = true;
                nodeType = item!.GetType();
            }
            else {
                //if (node is FileNode && item is FileNode )
                item!.IsSelected = nodeType == item.GetType();
                if (!item!.IsSelected) {
                    var d = e.SelectedIndexes[index];
                    explorerViewModel.ExplorerTree.RowSelection!.Deselect(d);
                }
            }
        }
    }
}