#region

using System.Linq;
using System.Reactive;
using System.Windows.Input;

using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.ViewModels.Footer;
using Tefin.ViewModels.MainMenu;
using Tefin.ViewModels.Misc;
using Tefin.ViewModels.Overlay;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    public FooterViewModel Footer { get; } = new();
    public MainMenuViewModel MainMenu { get; } = new();
    public MiscViewModel Misc { get; } = new();
    public OverlayHostViewModel Overlay { get; } = new();
    public AppTypes.Root? Root { get; private set; }
    public string SubTitle { get; } = "Native, cross-platform gRPC testing";
    public TabHostViewModel TabHost { get; } = new();
    public string Title { get; } = Core.Utils.appName;

    public ICommand SponsorCommand {
        get;
    }

    public MainWindowViewModel() {
        this.SponsorCommand = CreateCommand(this.OnSponsor);
        this.Root = default;
    }

    private void OnSponsor() {
        Core.Utils.openBrowser("https://github.com/sponsors/namigop"); 
    }

    public void Init() {
        this.Root = new StartupFeature().Load();
        var defaultPackage = this.Root.Packages.First(t => t.Name == Core.App.defaultPackage);
        this.MainMenu.ClientMenuItem.Init(defaultPackage, ProjectTypes.Project.DefaultName);
        this.MainMenu.ClientMenuItem.SelectItemCommand.Execute(Unit.Default);

        var hasClients = this.MainMenu.ClientMenuItem.Project.Clients.Any();
        if (hasClients) {
            foreach (var c in this.MainMenu.ClientMenuItem.Project.Clients) {
                Io.Log.Info($"Found client {c.Name}, {c.Config.Value.ServiceName}@{c.Config.Value.Url}");
                Io.Log.Warn("Recompile the client first to start testing the methods");
            }
        }
    }
}