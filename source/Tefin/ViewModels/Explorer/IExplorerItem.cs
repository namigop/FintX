#region

using System.Collections.ObjectModel;

#endregion

namespace Tefin.ViewModels.Explorer;

public interface IExplorerItem {
    bool CanOpen { get; }
    public bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    ObservableCollection<IExplorerItem> Items { get; }
    IExplorerItem Parent { get; set; }
    string SubTitle { get; set; }
    string Title { get; set; }

    IExplorerItem FindSelected();
}