using System.Collections.ObjectModel;
using System.Windows.Input;

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

        this.SelectedProject = this.RecentProjects.First();
            
        GlobalHub.subscribe<NewProjectCreatedMessage>(OnReceiveNewProjectCreatedMessage);
    }

    public ProjectSelection SelectedProject {
        get => this._selectedProject;
        set => this.RaiseAndSetIfChanged(ref _selectedProject, value);
    }

    private void OnReceiveNewProjectCreatedMessage(NewProjectCreatedMessage obj) {
        this._explorerViewModel.LoadProject(obj.ProjectPath);
    }

    public ObservableCollection<ProjectSelection> RecentProjects { get; set; }

    public ICommand OpenProjectCommand { get; set; }

    public ICommand NewProjectCommand { get; set; }

    private async Task OnOpenProject() {
        var proj = await DialogUtils.SelectFolder();
        this._explorerViewModel.LoadProject(proj);
    }

    private void OnNewProject() {
        var vm = new AddNewProjectOverlayViewModel(this._explorerViewModel.Project.Package);
        GlobalHub.publish(new OpenOverlayMessage(vm));
    }
}