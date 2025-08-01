﻿#region

using System.Reactive;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.ViewModels.Footer;
using Tefin.ViewModels.MainMenu;
using Tefin.ViewModels.Misc;
using Tefin.ViewModels.Overlay;
using Tefin.ViewModels.ProjectEnv;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    public MainWindowViewModel() {
        this.SponsorCommand = this.CreateCommand(this.OnSponsor);
        this.Root = default;
        this.MainMenu = new MainMenuViewModel();
        var appState = Core.App.getAppState(this.Io);
        this.EnvMenuViewModel = new EnvMenuViewModel();
        this.ProjectMenuViewModel = new ProjectMenuViewModel(
            this.MainMenu.ClientMenuItem.Explorer,
            this.MainMenu.ConfigMenuItem.Explorer, 
            this.EnvMenuViewModel, 
            appState);
    }

    public EnvMenuViewModel EnvMenuViewModel { get; set; }

    public FooterViewModel Footer { get; } = new();
    public MainMenuViewModel MainMenu { get; }
    public MiscViewModel Misc { get; } = new();
    public OverlayHostViewModel Overlay { get; } = new();
    public AppTypes.Root? Root { get; private set; }
    public string SponsorAlignment { get; } = Core.Utils.isMac() ? "Right" : "Left";

    public ICommand SponsorCommand { get; }

    public string SubTitle { get; } = "Native, cross-platform gRPC testing";
    public TabHostViewModel TabHost { get; } = new();
    public HostWindowViewModel WindowHost { get; } = new();
    public string Title { get; } = $"{Core.Utils.appName} v{Core.Utils.appVersionSimple}";
    public ProjectMenuViewModel ProjectMenuViewModel { get; }

    public void Init() {
        this.Root = new StartupFeature().Load(this.Io);
        var packageName = this.ProjectMenuViewModel.SelectedProject.Package;
        var projPath = this.ProjectMenuViewModel.SelectedProject.Path;

        var load = new LoadProjectFeature(this.Io, projPath);
        var project = load.Run();

        //var package = this.Root.Packages.First(t => t.Name == packageName);
        this.MainMenu.ClientMenuItem.Init(project);
        this.MainMenu.ServerMenuItem.Init();
        this.MainMenu.ConfigMenuItem.Init(project);
        this.MainMenu.InfoMenuItem.Init();
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

    private void OnSponsor() => Core.Utils.openBrowser("https://github.com/sponsors/namigop");

    private void StartAutoSave() {
        #if DEBUG
       // return;
        #endif

        AutoSave.ClientParam[] GetClientParam(List<PersistedTabViewModel> persistableTabs) {
            var loadedProject = this.MainMenu.ClientMenuItem.Explorer.Project;
            var loadedClients = this.MainMenu.ClientMenuItem.Explorer.GetClientNodes()
                .Where(c => c.IsLoaded)
                .Select(c => c.Client); //methodTabs.Select(m => m.Client).DistinctBy(c => c.Name).ToArray();

            var clientParams = new List<AutoSave.ClientParam>();
            foreach (var client in loadedClients) {
                var methodsOfClient = persistableTabs.Where(m => m.Client.Name == client.Name).ToArray();
                var uniqueMethods = methodsOfClient.DistinctBy(m => m.ClientMethod.MethodInfo.Name);
                var methodParams = new List<AutoSave.MethodParam>();
                foreach (var method in uniqueMethods) {
                    var tabsForMethod = methodsOfClient.Where(m =>
                        m.ClientMethod.MethodInfo.Name == method.ClientMethod.MethodInfo.Name);
                    var fileParams = new List<AutoSave.FileParam>();
                    foreach (var tab in tabsForMethod.Where(t => t.ClientMethod.IsLoaded)) {
                        var fp = tab.GetFileParam();
                        if (fp.CanSave)
                            fileParams.Add(fp);
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

        AutoSave.FileParam[] GetEnvConfigParam(PersistedTabViewModel[] nonMethodTabs) {
            return nonMethodTabs.Select(t => t.GetFileParam())
                .Where(t => t.CanSave)
                .ToArray();
        }
        
        AutoSave.AutoSaveParam[] Get() {
            var persistableTabs = this.TabHost.Items.Where(t => t.CanAutoSave).Cast<PersistedTabViewModel>().ToList();
            var openWindows = this.WindowHost.Items.Select(f => f.Value.Content).Where(t => t.CanAutoSave).Cast<PersistedTabViewModel>();
            persistableTabs.AddRange(openWindows);
            var nonMethodTabs = persistableTabs.Where(t => t.Client.Methods.Length == 0).ToArray();

            var clientParams = GetClientParam(persistableTabs);
            var fileParams = GetEnvConfigParam(nonMethodTabs);
            //return clientParams.ToArray();
            return [
                AutoSave.AutoSaveParam.AsClientParams(clientParams),
                AutoSave.AutoSaveParam.AsFileParams(fileParams),
            ];
        }

        AutoSave.run.Invoke(Get);
    }
}