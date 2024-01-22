using System.Collections.ObjectModel;
using System.Windows.Input;

using Tefin.Utils;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.ProjectEnv; 

public class ProjectMenuViewModel : ViewModelBase {
    private readonly ExplorerViewModel _explorerViewModel;

    public ProjectMenuViewModel(ExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.NewProjectCommand = CreateCommand(OnNewProject);
        this.OpenProjectCommand = CreateCommand(OnOpenProject);
        this.RecentProjects = new ObservableCollection<string>(); //read from app state file
        for (int i = 0; i < 5; i++) {
            this.RecentProjects.Add($"MyProject {i}");
        }
    }
    
    private async Task OnOpenProject() {
        var proj = await DialogUtils.SelectFolder();
        this._explorerViewModel.LoadProject(proj);
    }

    private void OnNewProject() {
        throw new NotImplementedException();
    }

    public ObservableCollection<string> RecentProjects { get; set; }

    public ICommand OpenProjectCommand { get; set; }

    public ICommand NewProjectCommand { get; set; }
}