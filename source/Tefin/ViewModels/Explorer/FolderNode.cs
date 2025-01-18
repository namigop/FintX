using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;

namespace Tefin.ViewModels.Explorer;

public class FolderNode : NodeBase {
    private string _icon;
    private bool _isCut;
    private string _tempTitle;

    protected FolderNode(string dirPath) {
        this.IsExpanded = true;
        this.FullPath = dirPath;
        base.Title = Path.GetFileName(dirPath);
        this._tempTitle = base.Title;
        this._icon = "Icon.Folder";
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        this.RenameCommand = this.CreateCommand(this.OnRename);
        this.CutCommand = this.CreateCommand(this.OnCut);
        this.CopyCommand = this.CreateCommand(this.OnCopy);
        this.PasteCommand = this.CreateCommand(this.OnPaste);

        this.SubscribeTo(vm => ((FolderNode)vm).IsExpanded, this.OnExpandedChanged);
    }

    public ICommand CutCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }
    public ICommand RenameCommand { get; private set; }

    public string TempTitle {
        get => this._tempTitle;
        set => this.RaiseAndSetIfChanged(ref this._tempTitle, value);
    }

    public override bool IsCut {
        get => this._isCut;
        set {
            this.RaiseAndSetIfChanged(ref this._isCut, value);
            foreach (var explorerItem in this.Items) {
                var i = (NodeBase)explorerItem;
                i.IsCut = value;
            }
        }
    }

    public string Icon {
        get => this._icon;
        private set => this.RaiseAndSetIfChanged(ref this._icon, value);
    }

    public string FullPath { get; private set; }
    public ICommand DeleteCommand { get; }

    private void OnCut() {
        this.IsCut = true;
        var msg = new CutCopyNodeMessage([this.FullPath], [this], false, true);
        GlobalHub.publish(msg);
    }

    private void OnCopy() {
        var msg = new CutCopyNodeMessage([this.FullPath], [this], false);
        GlobalHub.publish(msg);
    }

    private void OnPaste() {
        var msg = new PasteNodeMessage(this);
        GlobalHub.publish(msg);
    }

    private void OnDelete() {
        if (this.IsEditing) {
            return;
        }

        this.Io.Dir.Delete(this.FullPath, true);
    }

    private void OnRename() => this.IsEditing = true;

    private void OnExpandedChanged(ViewModelBase vm) =>
        this.Icon = ((FolderNode)vm).IsExpanded ? "Icon.FolderOpen2" : "Icon.Folder";

    public override void Init() {
        //Create the sub folders
        var subDirNames = this.Io.Dir.GetDirectories(this.FullPath).Select(d => Path.GetFileName(d)).Order().ToArray();

        foreach (var subDirName in subDirNames) {
            var subDir = Path.Combine(this.FullPath, subDirName);
            var msg = new FileChangeMessage(subDir, "", WatcherChangeTypes.Created);
            GlobalHub.publish(msg);
        }

        var fileNames = this.Io.Dir.GetFiles(this.FullPath, "*.*", SearchOption.TopDirectoryOnly).Select(c => Path.GetFileName(c)).Order().ToArray();

        //this will trigger the handlers that will create the file nodes
        foreach (var fileName in fileNames) {
            var file = Path.Combine(this.FullPath, fileName);
            var msg = new FileChangeMessage(file, "", WatcherChangeTypes.Created);
            GlobalHub.publish(msg);
        }
    }

    public void CancelEdit() {
        this.TempTitle = Path.GetFileName(this.FullPath);
        this.IsEditing = false;
    }

    public void EndEdit() =>
        this.Exec(() => {
            this.TempTitle = Core.Utils.makeValidFileName(this.TempTitle.Trim());

            if (this.TempTitle != this.Title) {
                this.Title = this.TempTitle;
                var newFolderPath = Path.GetDirectoryName(this.FullPath).Then(path => Path.Combine(path!, this.Title));
                if (!this.Io.Dir.Exists(newFolderPath)) {
                    //will trigger a file watcher event that will sync the explorer tree
                    this.Io.Dir.Move(this.FullPath, newFolderPath);
                }
                else {
                    this.Title = Path.GetFileName(this.FullPath);
                    this.Io.Log.Warn($"Unable to rename {Path.GetFileName(this.FullPath)}. Target file already exists");
                }
            }

            this.IsEditing = false;
        });

    public void UpdateFolderPath(string newFullPath) {
        this.Io.Log.Debug("Updated folder path");
        this.Io.Log.Debug($"--> (old) {this.FullPath}");
        this.Io.Log.Debug($"--> (new) {newFullPath}");
        this.FullPath = newFullPath;
        this.Title = Path.GetFileName(newFullPath);

        foreach (var child in this.Items) {
            if (child is FileNode fn) {
                var fileName = Path.GetFileName(fn.FullPath);
                var newFilePath = Path.Combine(newFullPath, fileName);
                fn.UpdateFilePath(newFilePath);
            }
        }

        // var clientPath = this.FindParentNode<ClientRootNode>()?.ClientPath ?? this.FindParentNode<FuncTestRootNode>()?.ClientPath;
        // if (!string.IsNullOrWhiteSpace(clientPath)) {
        //     var load = new LoadClientFeature(this.Io, clientPath);
        //     var newClientGroup = load.Run();
        //     var msg = new MessageProject.MsgClientUpdated(newClientGroup, clientPath, clientPath);
        //     GlobalHub.publish(msg);
        // }
    }
}