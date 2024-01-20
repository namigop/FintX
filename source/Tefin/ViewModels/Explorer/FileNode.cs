using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels.Explorer;

public class FileNode : NodeBase {
    private string _tempTitle;

    public FileNode(string fullPath) {
        this.CanOpen = true;
        this.FullPath = fullPath;
        this.Title = Path.GetFileName(fullPath);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        this.RenameCommand = this.CreateCommand(this.OnRename);
        this.OpenCommand = this.CreateCommand(this.OnOpen);
    }

    public ICommand DeleteCommand { get; }
    public string FullPath { get; private set; }
    public ICommand OpenCommand { get; }
    public ICommand RenameCommand { get; }

    public string TempTitle {
        get => this._tempTitle;
        set => this.RaiseAndSetIfChanged(ref this._tempTitle, value);
    }

    public void CancelEdit() {
        this.TempTitle = Path.GetFileName(this.FullPath);
        this.IsEditing = false;
    }

    public void EndEdit() {
        this.TempTitle = Core.Utils.makeValidFileName(this.TempTitle.Trim());
        var origExt = Path.GetExtension(this.FullPath);
        if (!this.TempTitle.EndsWith(origExt)) {
            this.TempTitle = $"{this.TempTitle}{origExt}";
        }

        if (this.TempTitle != this.Title) {
            this.Title = this.TempTitle;
            var newFile = Path.GetDirectoryName(this.FullPath).Then(path => Path.Combine(path, this.Title));
            if (!this.Io.File.Exists(newFile)) {
                //will trigger a file watcher event that will sync the explorer tree
                this.Io.File.Move(this.FullPath, newFile);
            }
            else {
                this.Title = Path.GetFileName(this.FullPath);
                this.Io.Log.Warn($"Unable to rename {Path.GetFileName(this.FullPath)}. Target file already exists");
            }
        }

        this.IsEditing = false;
    }

    public override void Init() {
    }

    public void UpdateFilePath(string newFilePath) {
        if (this.Io.File.Exists(newFilePath)) {
            this.FullPath = newFilePath;
            this.Title = Path.GetFileName(newFilePath);
        }
        else {
            this.Io.Log.Warn($"Unable to update file path. {newFilePath} does not exist");
        }
    }

    private void OnDelete() {
        // Delete from the file system *only*.  The OS event notification FileSystemWatcher
        // will get handled by ExplorerViewModel and the delete file will be removed
        // from the explorer tree
        this.Io.File.Delete(this.FullPath);
    }

    private void OnOpen() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null)
            GlobalHub.publish(new OpenTabMessage(tab));
    }

    private void OnRename() {
        this.IsEditing = true;
    }
}