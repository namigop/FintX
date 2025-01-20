using System.Collections.ObjectModel;

namespace Tefin.ViewModels.Explorer;

public interface IExplorerItem {
    bool CanOpen { get; }
    public bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    ObservableCollection<IExplorerItem> Items { get; }
    IExplorerItem? Parent { get; set; }
    string SubTitle { get; set; }
    string Title { get; set; }
    bool IsCut { get; set; }

    IExplorerItem? FindSelected();

    T? FindParentNode<T>(Func<T, bool>? filter = null) where T : IExplorerItem;
    IExplorerItem? FindChildNode(Func<IExplorerItem, bool> predicate);
    List<IExplorerItem> FindChildNodes(Func<IExplorerItem, bool> predicate);
}