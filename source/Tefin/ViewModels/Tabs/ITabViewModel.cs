#region

using System.Windows.Input;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface ITabViewModel {
    bool CanAutoSave { get; }
    ICommand CloseCommand { get; }
    ICommand CloseAllOthersCommand { get; }
    ICommand CloseAllCommand { get; }
    ICommand OpenInWindowCommand { get; }
    IExplorerItem ExplorerItem { get; }
    bool HasIcon { get; }
    string Icon { get; }
    string Id { get; }
    string SubTitle { get; set; }
    string Title { get; set; }
    bool IsSelected { get; set; }

    void Init();
}