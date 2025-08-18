using System.Windows.Input;
using ReactiveUI;

namespace Tefin.ViewModels.MainMenu;

public class InfoMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    private string _message;

    public InfoMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.OpenBrowserCommand = this.CreateCommand<string>(this.OnOpenBrowser);
        this.OpenGithubCommand = this.CreateCommand(this.OnOpenGithub);
    }
     
    public string AppInfo { get; } = $"{Core.Utils.appName} v{Core.Utils.appVersionSimple}";
    public string Copyright { get; } = "Copyright : Erik Araojo";
    //public ClientExplorerViewModel Explorer { get; set; } = new();
    public string GitHubUrl { get; } = "https://github.com/namigop/FintX";
    public string SiteUrl { get; } = "https://fintx.dev";
    public string OpenSourceLicense { get; } = "GNU General Public License v3.0";

    public ICommand OpenBrowserCommand { get; }
    public ICommand OpenGithubCommand { get; }
    public override string ToolTip { get; } = "Application info";
    
    public override string Name { get; } = "Info";
    public override string ShortName { get; } = "info";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    public string Message {
        get => this._message;
        private set => this.RaiseAndSetIfChanged(ref _message, value);
    }

    private void OnOpenBrowser(string url) => Core.Utils.openBrowser(url);

    private void OnOpenGithub() => Core.Utils.openBrowser(this.GitHubUrl);

    public void Init() {
    }
}