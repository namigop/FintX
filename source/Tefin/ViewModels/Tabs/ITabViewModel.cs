#region

using System.Windows.Input;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface ITabViewModel {
    bool CanAutoSave { get; }
    ICommand CloseAllCommand { get; }
    ICommand CloseAllOthersCommand { get; }
    ICommand CloseCommand { get; }
    IExplorerItem ExplorerItem { get; }
    bool HasIcon { get; }
    string Icon { get; }
    string Id { get; }
    bool IsSelected { get; set; }
    ICommand OpenInWindowCommand { get; }
    string SubTitle { get; set; }
    string Title { get; set; }

    void Init();
}