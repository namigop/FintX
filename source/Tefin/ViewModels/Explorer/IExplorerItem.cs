#region

using System.Collections.ObjectModel;

#endregion

namespace Tefin.ViewModels.Explorer;

public interface IExplorerItem {
    bool CanOpen { get; }
    public bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    ObservableCollection<IExplorerItem> Items { get; }
    string SubTitle { get; set; }
    string Title { get; set; }
    IExplorerItem Parent { get; set; }
}