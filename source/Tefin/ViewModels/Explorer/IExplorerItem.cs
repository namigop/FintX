using System.Collections.ObjectModel;

namespace Tefin.ViewModels.Explorer;

public interface IExplorerItem {
    bool CanOpen { get; }
    bool IsCut { get; set; }
    public bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    ObservableCollection<IExplorerItem> Items { get; }
    IExplorerItem? Parent { get; set; }
    string SubTitle { get; set; }
    string Title { get; set; }
    IExplorerItem? FindChildNode(Func<IExplorerItem, bool> predicate);
    List<IExplorerItem> FindChildNodes(Func<IExplorerItem, bool> predicate);

    T? FindParentNode<T>(Func<T, bool>? filter = null) where T : IExplorerItem;

    IExplorerItem? FindSelected();
}