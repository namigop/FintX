using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.Config;
using Tefin.ViewModels.Explorer.ServiceMock;
using Tefin.ViewModels.Overlay;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectMenuViewModel : ViewModelBase {
    private readonly ClientExplorerViewModel _clientExplorer;
    private readonly ConfigExplorerViewModel _configExplorer;
    private readonly ServiceMockExplorerViewModel _serviceMockExplorer;
    private readonly EnvMenuViewModel _envMenu;
    private ProjectSelection _selectedProject;

    public ProjectMenuViewModel(
        ClientExplorerViewModel explorerViewModel,
        ConfigExplorerViewModel configExplorer, 
        ServiceMockExplorerViewModel serviceMockExplorer,
        EnvMenuViewModel envMenu, AppTypes.AppState? appState) {
        this._clientExplorer = explorerViewModel;
        this._configExplorer = configExplorer;
        this._serviceMockExplorer = serviceMockExplorer;
        this._envMenu = envMenu;
        this.NewProjectCommand = this.CreateCommand(this.OnNewProject);
        this.OpenProjectCommand = this.CreateCommand(this.OnOpenProject);
        this.RecentProjects = new ObservableCollection<ProjectSelection>();

        //read from app state file
        var defaultProject = Core.App.getDefaultProjectPath(Core.App.defaultPackage);
        if (appState == null || appState.RecentProjects.Length == 0) {
            this.RecentProjects.Add(new ProjectSelection(Core.App.defaultPackage, defaultProject));
            this._selectedProject = this.RecentProjects.First();
            this._selectedProject.IsSelected = true;
        }
        else {
            foreach (var proj in appState.RecentProjects) {
                var selection = new ProjectSelection(proj.Package, proj.Path);
                this.RecentProjects.Add(selection);
            }

            this._selectedProject = this.RecentProjects.First(f => f.Path == appState.ActiveProject.Path);
            this._selectedProject.IsSelected = true;
            this._envMenu.Init(this._selectedProject.Path);
        }
        
        GlobalHub.publish(new ProjectSelectedMessage(this._selectedProject.Name,
            this._selectedProject.Path, this._selectedProject.Package));

        GlobalHub.subscribe<NewProjectCreatedMessage>(this.OnReceiveNewProjectCreatedMessage)
            .Then(this.MarkForCleanup);
        this.SubscribeTo(vm => ((ProjectMenuViewModel)vm).SelectedProject, this.OnSelectedProjectChanged);
    }

    public ProjectSelection SelectedProject {
        get => this._selectedProject;
        set {
            if (!Directory.Exists(value.Path)) {
                this.Io.Log.Warn($"Project folder no longer exist. {value.Path}");
                this.RecentProjects.Remove(value);
                return;
            }

            this.RaiseAndSetIfChanged(ref this._selectedProject, value);
        }
    }

    public ObservableCollection<ProjectSelection> RecentProjects { get; set; }

    public ICommand OpenProjectCommand { get; set; }

    public ICommand NewProjectCommand { get; set; }

    private void OnSelectedProjectChanged(ViewModelBase obj) =>
        this.Exec(() => {
            var vm = (ProjectMenuViewModel)obj;
            foreach (var i in vm.RecentProjects) {
                i.IsSelected = i == vm.SelectedProject;
            }

            vm.OpenProject(vm.SelectedProject.Path);
            this._envMenu.Init(vm.SelectedProject.Path);
            GlobalHub.publish(new ProjectSelectedMessage(vm.SelectedProject.Name, vm.SelectedProject.Path, vm.SelectedProject.Package));
        });

    private void OnReceiveNewProjectCreatedMessage(NewProjectCreatedMessage obj) =>
        this.Exec(() => {
            this.ResetExplorers(obj.ProjectPath);
            if (!this.RecentProjects.Contains(i => i.Path == obj.ProjectPath)) {
                var projSelection = new ProjectSelection(obj.Package, obj.ProjectPath);
                this.RecentProjects.Add(projSelection);
                this.SelectedProject = projSelection;
            }
        });

    private async Task OnOpenProject() {
        var projectPath = await DialogUtils.SelectFolder();
        this.OpenProject(projectPath);
    }
    private void ResetExplorers(string projectPath) {
        this._clientExplorer.LoadProject(projectPath);
        this._serviceMockExplorer.Project = this._clientExplorer.Project;
        this._configExplorer.Project = this._clientExplorer.Project;
        this._configExplorer.Clear();
        this._configExplorer.Init();
        
    }

    private void OpenProject(string projectPath) {
        this.ResetExplorers(projectPath);
        if (!this.RecentProjects.Contains(i => i.Path == this._clientExplorer.Project!.Path)) {
            var pack = this._clientExplorer.Project!.Package;
            var path = this._clientExplorer.Project!.Path;
            var projSelection = new ProjectSelection(pack, path);
            this.RecentProjects.Add(projSelection);
            this.SelectedProject = projSelection;
        }
    }

    private void OnNewProject() {
        var pack = this._clientExplorer.Project?.Package ?? Core.App.defaultPackage;
        var vm = new AddNewProjectOverlayViewModel(pack);
        GlobalHub.publish(new OpenOverlayMessage(vm));
    }
}