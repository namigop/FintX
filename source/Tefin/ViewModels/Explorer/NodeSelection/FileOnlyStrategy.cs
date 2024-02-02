using Avalonia.Controls;
using Avalonia.Controls.Selection;

namespace Tefin.ViewModels.Explorer;

/// <summary>
/// Single selection for non-FileNodes.  MultipleSelection for FileNodes 
/// </summary>
/// <param name="explorerViewModel"></param>
public class FileOnlyStrategy(ExplorerViewModel explorerViewModel) : IExplorerNodeSelectionStrategy {
    public void Apply(TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        var selected = explorerViewModel.GetClientNodes()
            .Select(c => c.FindSelected())
            .FirstOrDefault(m => m != null);

        List<IExplorerItem> selectedItems = new();
        for (int i = 0; i < e.SelectedItems.Count; i++)
            selectedItems.Add(e.SelectedItems[i]!);
        
        List<IndexPath> selectedIndexes = new();
        for (int i = 0; i < e.SelectedIndexes.Count; i++)
            selectedIndexes.Add(e.SelectedIndexes[i]);
        
        int index = -1;
        foreach (var item in selectedItems) {
            index += 1;

            if (item == null)
                continue;
            if (selected == null) {
                item.IsSelected = true;
                selected = item;
                continue;
            }

            if (selected is FileNode && item is FileNode fn) {
                var p1 = selected.FindParentNode<ClientNode>();
                var p2 = item.FindParentNode<ClientNode>();

                if (p1 == p2) {
                    fn.IsSelected = true;
                    selected = fn;
                    continue;
                }
            }

            var d = selectedIndexes[index];
            explorerViewModel.ExplorerTree.RowSelection!.Deselect(d);
        }

    }
}