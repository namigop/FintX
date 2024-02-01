using Avalonia.Controls.Selection;

namespace Tefin.ViewModels.Explorer;

public interface IExplorerNodeSelectionStrategy {
    void Apply(TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e);
}