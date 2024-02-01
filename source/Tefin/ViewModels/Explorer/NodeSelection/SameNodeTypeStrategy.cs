using Avalonia.Controls.Selection;

namespace Tefin.ViewModels.Explorer;

/// <summary>
/// Multiple selection allowed only for the types of nodes
/// </summary>
/// <param name="explorerViewModel"></param>
public class SameNodeTypeStrategy(ExplorerViewModel explorerViewModel) : IExplorerNodeSelectionStrategy {
    public void Apply(TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        var selected = explorerViewModel.GetClientNodes()
            .Select(c => c.FindSelected())
            .FirstOrDefault(m => m != null);
        var nodeType = selected?.GetType();
        int index = -1;
        foreach (var item in e.SelectedItems) {
            index += 1;
            if (item == null)
                continue;
            if (nodeType == null) {
                item!.IsSelected = true;
                nodeType = item!.GetType();
            }
            else {
                item!.IsSelected = nodeType == item.GetType();
                if (!item!.IsSelected) {
                    var d = e.SelectedIndexes[index];
                    explorerViewModel.ExplorerTree.RowSelection!.Deselect(d);
                }
            }
        }
    }
}