#region

using System.Windows.Input;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface ITabViewModel {
    bool CanAutoSave { get; }
    ICommand CloseCommand { get; }
    IExplorerItem ExplorerItem { get; }
    string Id { get; }
    string SubTitle { get; set; }
    string Title { get; set; }
    string Icon { get; }

    bool HasIcon { get; }
    void Init();
}