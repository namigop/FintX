#region

using System.Linq;
using System.Reactive;
using System.Windows.Input;

using Tefin.Core;
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
    public MainWindowViewModel() {
        this.SponsorCommand = this.CreateCommand(this.OnSponsor);
        this.Root = default;
    }
    public FooterViewModel Footer { get; } = new();
    public MainMenuViewModel MainMenu { get; } = new();
    public MiscViewModel Misc { get; } = new();
    public OverlayHostViewModel Overlay { get; } = new();
    public AppTypes.Root? Root { get; private set; }
    public string SubTitle { get; } = "Native, cross-platform gRPC testing";
    public TabHostViewModel TabHost { get; } = new();
    public string Title { get; } = $"{Core.Utils.appName} {Core.Utils.appVersionSimple}";

    public ICommand SponsorCommand {
        get;
    }

    public string SponsorAlignment { get; } = Core.Utils.isWindows() ? "Left" : "Right";

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
                this.Io.Log.Info($"Found client {c.Name}, {c.Config.Value.ServiceName}@{c.Config.Value.Url}");
                this.Io.Log.Warn("Recompile the client first to start testing the methods");
            }
        }

        this.StartAutoSave();
    }

    private void StartAutoSave() {
        AutoSave.ClientParam[] Get() {
            var methodTabs = this.TabHost.Items
                .Where(t => t is MethodTabViewModel)
                .Cast<MethodTabViewModel>()
                .ToArray();
            var loadedProject = this.MainMenu.ClientMenuItem.Explorer.Project;

            var loadedClients = methodTabs.Select(m => m.Client).DistinctBy(c => c.Name).ToArray();
            var clientParams = new List<AutoSave.ClientParam>();
            foreach (var client in loadedClients) {
                var methodsOfClient = methodTabs.Where(m => m.Client.Name == client.Name).ToArray();
                var uniqueMethods = methodsOfClient.DistinctBy(m => m.ClientMethod.MethodInfo.Name);
                var methodParams = new List<AutoSave.MethodParam>();
                foreach (var method in uniqueMethods) {
                    var tabsForMethod = methodsOfClient.Where(m => m.ClientMethod.MethodInfo.Name == method.ClientMethod.MethodInfo.Name);
                    var fileParams = new List<AutoSave.FileParam>();
                    foreach (var tab in tabsForMethod) {
                        var json = tab.GetRequestContent();
                        var title = tab.Title;
                        var fileParam = AutoSave.FileParam.Empty()
                            .WithJson(json)
                            .WithHeader(title);
                        fileParams.Add(fileParam);
                    }

                    var methodParam = AutoSave.MethodParam.Empty()
                        .WithName(method.ClientMethod.MethodInfo.Name)
                        .WithFiles(fileParams.ToArray());
                    methodParams.Add(methodParam);
                }

                var clientParam =
                    AutoSave.ClientParam.Empty()
                        .WithProject(loadedProject)
                        .WithClient(client)
                        .WithMethods(methodParams.ToArray());
                clientParams.Add(clientParam);
            }

            return clientParams.ToArray();
        }

        AutoSave.run.Invoke(Get);
    }
}