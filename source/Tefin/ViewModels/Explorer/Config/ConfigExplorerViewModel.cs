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
                    (i, msg) => fileChange.Create(i, msg, path => new EnvNode(path), clientPath => "TODO :uatpath"),
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

    public ICommand EditCommand { get; }

    protected override string[] SupportedExtensions { get; }

    protected override MultiNodeFile CreateMultiNodeFile(IExplorerItem[] items, ProjectTypes.ClientGroup client) => throw new NotImplementedException();

    protected override NodeBase CreateMultiNodeFolder(IExplorerItem[] items, ProjectTypes.ClientGroup client) => throw new NotImplementedException();

    protected override string GetRootFilePath(string clientPath) => throw new NotImplementedException();
    protected override ConfigGroupNode CreateRootNode(ProjectTypes.ClientGroup cg, Type? type = null) => throw new NotImplementedException();

    private void OnEdit() { }

    public void Init() {
        var envGroup = new ConfigGroupNode { Title = "Environments", SubTitle = "All environments" };
        var devEnv = new EnvNode("TODOPath") { Title = "DEV", SubTitle = "Development environments" };
        var uatEnv = new EnvNode("TODOPath") { Title = "UAT", SubTitle = "UAT environments" };
        var prodEnv = new EnvNode("TODOPath") { Title = "PROD", SubTitle = "Production environments" };
        envGroup.AddItem(devEnv);
        envGroup.AddItem(uatEnv);
        envGroup.AddItem(prodEnv);
        this.Items.Add(envGroup);
    }
}