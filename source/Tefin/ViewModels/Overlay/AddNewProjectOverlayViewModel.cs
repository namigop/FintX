using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;

namespace Tefin.ViewModels.Overlay;

public class AddNewProjectOverlayViewModel : ViewModelBase, IOverlayViewModel {
    private readonly string _package;
    private string _parentFolder = "";
    private string _projectName = "";
    public string Title { get; } = "Create new project";

    public AddNewProjectOverlayViewModel(string package) {
        this._package = package;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.SelectFolderCommand = this.CreateCommand((this.OnSelectFolder));
            
        this.ParentFolder = "";
        this.ProjectName = "";
    }

    private async Task OnSelectFolder() {
        this.ParentFolder = await DialogUtils.SelectFolder();
    }

    public string ParentFolder {
        get => this._parentFolder;
        set => this.RaiseAndSetIfChanged(ref this._parentFolder, value);
    }

    public string ProjectName {
        get => this._projectName;
        set {
            var dir = Core.Utils.makeValidFileName(value);
            this.RaiseAndSetIfChanged(ref this._projectName, dir);
        }
    }

    private void OnOkay() {
        if (string.IsNullOrWhiteSpace(this.ParentFolder))
            return;
        
        if (string.IsNullOrWhiteSpace(this.ProjectName))
            return;

        if (!this.Io.Dir.Exists(this._parentFolder)) {
            this.Io.Log.Error($"{this.ParentFolder} does not exist. Please select another folder");
            return;
        }

        var projectPath = Path.Combine(this.ParentFolder, this.ProjectName);
        this.Io.Dir.CreateDirectory(projectPath);

        Core.Project.createSaveState(this.Io, this._package, projectPath);
        GlobalHub.publish(new NewProjectCreatedMessage(projectPath));
        this.Close();
    }

    public ICommand OkayCommand { get; }

    public ICommand CancelCommand { get; }
    public ICommand SelectFolderCommand { get; }

    public void Close() { GlobalHub.publish(new CloseOverlayMessage(this)); }
}