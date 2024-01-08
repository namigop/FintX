#region

using System.Windows.Input;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface ITabViewModel {
    bool AllowDuplicates { get; set; }
    ICommand CloseCommand { get; }
    IExplorerItem ExplorerItem { get; }
    string Id { get; }
    string SubTitle { get; set; }
    string Title { get; set; }

    void Init();

    void Import(string requestFile);
}