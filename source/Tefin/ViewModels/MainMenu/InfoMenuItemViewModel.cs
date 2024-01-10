using System.Windows.Input;

using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.MainMenu;

public class InfoMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public InfoMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.OpenBrowserCommand = this.CreateCommand(this.OnOpenBrowser);
    }

    public ICommand OpenBrowserCommand { get; }

    public ExplorerViewModel Explorer { get; set; } = new();
    public override string ToolTip { get; } = "Application info";

    public string AppInfo { get; } = $"{Core.Utils.appName} v{Core.Utils.appVersionSimple}";

    public string GitHubUrl { get; } = "https://github.com/namigop/FintX";

    public string License { get; } = "GNU General Public License v3.0";

    public string Copyright { get; } = "Copyright : Erik Araojo";
    public override string Name { get; } = "Info";
    public override string ShortName { get; } = "info";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    private void OnOpenBrowser() {
        Core.Utils.openBrowser(this.GitHubUrl);
    }
}