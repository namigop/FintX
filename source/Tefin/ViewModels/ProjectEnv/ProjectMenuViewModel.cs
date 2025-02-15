using System.Collections.ObjectModel;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.Config;
using Tefin.ViewModels.Overlay;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectMenuViewModel : ViewModelBase {
    private readonly ClientExplorerViewModel _explorerViewModel;
    private readonly EnvMenuViewModel _envMenu;
    private ProjectSelection _selectedProject;

    public ProjectMenuViewModel(ClientExplorerViewModel explorerViewModel, EnvMenuViewModel envMenu, AppTypes.AppState? appState) {
        this._explorerViewModel = explorerViewModel;
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
        });

    private void OnReceiveNewProjectCreatedMessage(NewProjectCreatedMessage obj) =>
        this.Exec(() => {
            this._explorerViewModel.LoadProject(obj.ProjectPath);
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

    private void OpenProject(string projectPath) {
        this._explorerViewModel.LoadProject(projectPath);
        if (!this.RecentProjects.Contains(i => i.Path == this._explorerViewModel.Project!.Path)) {
            var pack = this._explorerViewModel.Project!.Package;
            var path = this._explorerViewModel.Project!.Path;
            var projSelection = new ProjectSelection(pack, path);
            this.RecentProjects.Add(projSelection);
            this.SelectedProject = projSelection;
        }
    }

    private void OnNewProject() {
        var pack = this._explorerViewModel.Project?.Package ?? Core.App.defaultPackage;
        var vm = new AddNewProjectOverlayViewModel(pack);
        GlobalHub.publish(new OpenOverlayMessage(vm));
    }
}