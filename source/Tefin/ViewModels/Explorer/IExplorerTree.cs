using Avalonia.Controls;

namespace Tefin.ViewModels.Explorer;

public interface IExplorerTree<T> where T : NodeBase {
    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; }
    public void Clear();
    public T[] GetRootNodes();
}