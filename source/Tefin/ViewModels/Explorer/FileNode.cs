using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Git;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels.Explorer;

public class FileNode : NodeBase {
    private FileGitStatus _gitFileStatus = FileGitStatus.NoRepository;
    private string _tempTitle = "";

    protected FileNode(string fullPath) {
        this.CanOpen = true;
        this.FullPath = fullPath;
        base.Title = Path.GetFileName(fullPath);
        this.TempTitle = base.Title;
        this.UpdateSubTitle();

        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        this.RenameCommand = this.CreateCommand(this.OnRename);
        this.OpenCommand = this.CreateCommand(this.OnOpen);
        this.OpenFilePathCommand = this.CreateCommand(this.OnOpenFilePath);
        this.OpenMethodInWindowCommand = this.CreateCommand(this.OpenMethodInWindow);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);

        this.CheckGitStatus();
    }

    public ICommand DeleteCommand { get; }
    public string FullPath { get; private set; }
    public string GitFileIcon => this.GetGitFileIcon();

    public FileGitStatus GitFileStatus {
        get => this._gitFileStatus;
        private set {
            this.RaiseAndSetIfChanged(ref this._gitFileStatus, value, nameof(this.IsManagedByGit));
            this.RaisePropertyChanged(nameof(this.GitFileIcon));
        }
    }

    public bool IsManagedByGit => !this.GitFileStatus.IsNoRepository;

    public DateTime LastWriteTime => this.Io.File.GetLastWriteTime(this.FullPath);
    public ICommand OpenCommand { get; }

    public ICommand OpenFilePathCommand { get; }
    public ICommand OpenMethodInWindowCommand { get; }
    public ICommand RenameCommand { get; }

    public string TempTitle {
        get => this._tempTitle;
        set => this.RaiseAndSetIfChanged(ref this._tempTitle, value);
    }

    public void CancelEdit() {
        this.TempTitle = Path.GetFileName(this.FullPath);
        this.IsEditing = false;
    }

    public void CheckGitStatus() => this.GitFileStatus = Git.getFileStatus(this.FullPath);

    public void EndEdit() =>
        this.Exec(() => {
            this.TempTitle = Core.Utils.makeValidFileName(this.TempTitle.Trim());
            var origExt = Path.GetExtension(this.FullPath);
            if (!this.TempTitle.EndsWith(origExt)) {
                this.TempTitle = $"{this.TempTitle}{origExt}";
            }

            if (this.TempTitle != this.Title) {
                this.Title = this.TempTitle;
                var newFile = Path.GetDirectoryName(this.FullPath).Then(path => Path.Combine(path!, this.Title));
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
        });

    private string GetGitFileIcon() {
        if (this.GitFileStatus.IsModified) {
            return "Icon.GitCheckSmall";
        }

        if (this.GitFileStatus.IsRenamed || this.GitFileStatus.IsAdded) {
            return "Icon.GitPlusSmall";
        }

        if (this.GitFileStatus.IsNoRepository) {
            return "Icon.GitEmptySmall";
        }

        if (this.GitFileStatus.IsIgnored || this.GitFileStatus.IsUntracked) {
            return "Icon.GitQuestionSmall";
        }

        return "Icon.GitLockSmall";

        // if (this.GitFileStatus.IsModified) {
        //     return GitStatusColors.Modified;
        // }
        // if (this.GitFileStatus.IsRenamed) {
        //     return GitStatusColors.Renamed;
        // }
        //     
        // if (this.GitFileStatus.IsAdded) {
        //     return GitStatusColors.Added;
        // }
        //     
        // if (this.GitFileStatus.IsIgnored) {
        //     return GitStatusColors.Ignored;
        // }
        //     
        // if (this.GitFileStatus.IsUntracked) {
        //     return GitStatusColors.Untracked;
        // }
        //     
        // return GitStatusColors.Default;
    }

    public override void Init() {
    }

    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        if (this.FullPath.StartsWith(obj.PreviousPath)) {
            this.UpdateFilePath(this.FullPath.Replace(obj.PreviousPath, obj.Path));
        }
    }

    private void OnDelete() =>
        // Delete from the file system *only*.  The OS event notification FileSystemWatcher
        // will get handled by ExplorerViewModel and the delete file will be removed
        // from the explorer tree
        this.Io.File.Delete(this.FullPath);

    private void OnOpen() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null) {
            this.Io.Log.Info($"Opening {this.FullPath}");
            GlobalHub.publish(new OpenTabMessage(tab));
        }
    }

    private void OnOpenFilePath() {
        var dir = Path.GetDirectoryName(this.FullPath);
        Core.Utils.openPath(dir);
    }

    private void OnRename() => this.IsEditing = true;

    private void OpenMethodInWindow() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null) {
            GlobalHub.publish(new OpenChildWindowMessage(tab));
        }
    }

    public void UpdateFilePath(string newFilePath) {
        if (this.Io.File.Exists(newFilePath)) {
            this.Io.Log.Debug($"Updated fileReqNode : {this.FullPath} -> {newFilePath}");
            this.FullPath = newFilePath;
            this.Title = Path.GetFileName(newFilePath);
            this.UpdateSubTitle();
        }
        else {
            this.Io.Log.Warn($"Unable to update file path. {newFilePath} does not exist");
        }
    }

    private void UpdateSubTitle() {
        var time = this.LastWriteTime.ToString("dddd, dd MMMM yyyy h:m tt");
        this.SubTitle = $"Last updated: {time}";
    }
}