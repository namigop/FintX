using System.Collections.ObjectModel;
using System.Windows.Input;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Overlay;

namespace Tefin.ViewModels.ProjectEnv;

public class ProjectMenuViewModel : ViewModelBase {
    private readonly ExplorerViewModel _explorerViewModel;
    private ProjectSelection _selectedProject;

    public ProjectMenuViewModel(ExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.NewProjectCommand = CreateCommand(OnNewProject);
        this.OpenProjectCommand = CreateCommand(OnOpenProject);
        this.RecentProjects = new ObservableCollection<ProjectSelection>();

        //read from app state file
        this.RecentProjects.Add(new ProjectSelection(Core.App.getDefaultProjectPath(Core.App.defaultPackage)));

        this._selectedProject = this.RecentProjects.First();
        this._selectedProject.IsSelected = true;
        GlobalHub.subscribe<NewProjectCreatedMessage>(OnReceiveNewProjectCreatedMessage);
        this.SubscribeTo(vm => ((ProjectMenuViewModel)vm).SelectedProject, OnSelectedProjectChanged);
    }

    private void OnSelectedProjectChanged(ViewModelBase obj) {
        var vm = (ProjectMenuViewModel)obj;
        foreach (var i in vm.RecentProjects)
            i.IsSelected = i == vm.SelectedProject;

        vm.OpenProject(vm.SelectedProject.Path);
    }

    public ProjectSelection SelectedProject {
        get => this._selectedProject;
        set {
            this.RaiseAndSetIfChanged(ref _selectedProject, value);
        }
    }

    private void OnReceiveNewProjectCreatedMessage(NewProjectCreatedMessage obj) {
        this._explorerViewModel.LoadProject(obj.ProjectPath);
        if (!this.RecentProjects.Contains(i => i.Path == obj.ProjectPath)) {
            var projSelection = new ProjectSelection(obj.ProjectPath);
            this.RecentProjects.Add(projSelection);
            this.SelectedProject = projSelection;
        }
    }

    public ObservableCollection<ProjectSelection> RecentProjects { get; set; }

    public ICommand OpenProjectCommand { get; set; }

    public ICommand NewProjectCommand { get; set; }

    private async Task OnOpenProject() {
        var projectPath = await DialogUtils.SelectFolder();
        this.OpenProject(projectPath);
    }

    private void OpenProject(string projectPath) {
        this._explorerViewModel.LoadProject(projectPath);
        if (!this.RecentProjects.Contains(i => i.Path == this._explorerViewModel.Project!.Path)) {
            var projSelection = new ProjectSelection(this._explorerViewModel.Project!.Path);
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