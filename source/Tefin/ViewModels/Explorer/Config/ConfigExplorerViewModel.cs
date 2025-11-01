using System.Windows.Input;

using Avalonia.Threading;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;

namespace Tefin.ViewModels.Explorer.Config;

using fileChange = FileChange<ConfigGroupNode, EnvNode>;

public class ConfigExplorerViewModel : ExplorerViewModel<ConfigGroupNode> {
    private static readonly object FileSystemHandlerLock = new();

    public ConfigExplorerViewModel() {
        this.SupportedExtensions = [Ext.envExt];
        this.EditCommand = this.CreateCommand(this.OnEdit);
        GlobalHub.subscribe<FileChangeMessage>(this.OnFileChanged).Then(this.MarkForCleanup);
    }

    public ICommand EditCommand { get; }

    protected override string[] SupportedExtensions { get; }

    protected override MultiNodeFile CreateMultiNodeFile(IExplorerItem[] items, ProjectTypes.ClientGroup client) =>
        throw new NotImplementedException();

    protected override NodeBase CreateMultiNodeFolder(IExplorerItem[] items, ProjectTypes.ClientGroup client) =>
        throw new NotImplementedException();

    protected override ConfigGroupNode CreateRootNode(ProjectTypes.ClientGroup cg, Type? type = null) =>
        new(this.Project!.Path);

    protected override string GetRootFilePath(string clientPath) => VarsStructure.getVarPathForProject(clientPath);

    public void Init() {
        if (this.Project is null) {
            return;
        }

        Dispatcher.UIThread.Invoke(() => {
            var root = new ConfigGroupNode(this.Project.Path) {
                Title = "Project Variables", SubTitle = "All environment variables for this project"
            };
            this.Items.Add(root);
            root.Init();
            root.IsExpanded = true;
        });
    }

    private void OnEdit() { }

    private void OnFileChanged(FileChangeMessage obj) {
        lock (FileSystemHandlerLock) {
            Dispatcher.UIThread.Invoke(() => this.OnFileChangedInternal(obj));
        }
    }

    private void OnFileChangedInternal(FileChangeMessage obj) {
        var ext = Path.GetExtension(obj.FullPath);
        if (ext == Ext.envExt) {
            //only handle the file changes if it is an extension that is used by FintX
            if (obj.ChangeType == WatcherChangeTypes.Changed) {
                NodeWalker.Walk(this.Items.ToArray(), obj, fileChange.FileModified, i => i.Items.Count == 0);
            }

            if (obj.ChangeType == WatcherChangeTypes.Deleted) {
                NodeWalker.Walk(this.Items.ToArray(), obj, fileChange.Delete, i => i.Items.Count == 0);
            }

            if (obj.ChangeType == WatcherChangeTypes.Renamed) {
                NodeWalker.Walk(this.Items.ToArray(), obj, fileChange.Rename, i => i is EnvNode);
            }

            if (obj.ChangeType == WatcherChangeTypes.Created) {
                NodeWalker.Walk(this.Items.ToArray(),
                    obj,
                    (i, msg) => fileChange.Create(i, msg, path => new EnvNode(path),
                        VarsStructure.getVarPathForProject),
                    i => i is ConfigGroupNode);
            }
        }

        //Handler for folder changes
        // if (ext == "") {
        //     var rootNode = this.GetRootNodes().FirstOrDefault(c => obj.FullPath.StartsWith(c.ClientPath));
        //     if (rootNode != null) {
        //         var basePath = FuncTestStructure.getTestsPath(rootNode.ClientPath);
        //         var folderChange = new FolderChange<FuncTestFolderNode>(rootNode, basePath);
        //         if (obj.ChangeType == WatcherChangeTypes.Deleted) {
        //             NodeWalker.Walk(this.Items.ToArray(), obj, folderChange.Delete, i => i is FolderNode);
        //         }
        //
        //         if (obj.ChangeType == WatcherChangeTypes.Renamed) {
        //             NodeWalker.Walk(this.Items.ToArray(), obj, folderChange.Rename, i => i is FolderNode);
        //         }
        //
        //         if (obj.ChangeType == WatcherChangeTypes.Created) {
        //             NodeWalker.Walk(this.Items.ToArray(), obj, (i, msg) => folderChange.Create(i, msg, path => new FuncTestFolderNode(path)), i => i is FuncTestRootNode);
        //         }
        //     }
        // }
    }
}