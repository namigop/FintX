#region

using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Overlay;

using static Tefin.Core.Interop.ProjectTypes;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;
using Type = System.Type;

#endregion

namespace Tefin.ViewModels.Explorer.Client;

public class ClientExplorerViewModel : ViewModelBase {
    private static readonly object FileSystemHandlerLock = new();
    private readonly IExplorerNodeSelectionStrategy _nodeSelectionStrategy;
    private CopyPasteArg? _copyPastePending;
    private Project? _project;

    public ClientExplorerViewModel() {
        this._nodeSelectionStrategy = new FileOnlyStrategy(this);
        var temp = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new TemplateColumn<IExplorerItem>("", "CellTemplate",
                        null, //edittemplate
                        new GridLength(1, GridUnitType.Star)), //
                    x => x.Items, //
                    x => x.Items.Any(), //
                    x => x.IsExpanded),
                new TemplateColumn<IExplorerItem>("", "CellActionTemplate", null, //edittemplate
                    new GridLength(1, GridUnitType.Auto))
            }
        };

        this.ExplorerTree = temp;
        this.ExplorerTree.RowSelection!.SingleSelect = false;
        this.ExplorerTree.RowSelection!.SelectionChanged += this.RowSelectionChanged;
        GlobalHub.subscribeTask<ShowClientMessage>(this.OnShowClient).Then(this.MarkForCleanup);
        GlobalHub.subscribe<ClientDeletedMessage>(this.OnClientDeleted).Then(this.MarkForCleanup);
        GlobalHub.subscribe<FileChangeMessage>(this.OnFileChanged).Then(this.MarkForCleanup);
        GlobalHub.subscribe<ClientCompileMessage>(this.OnClientCompile).Then(this.MarkForCleanup);

        this.CopyCommand = this.CreateCommand(this.OnCopy);
        this.PasteCommand = this.CreateCommand(this.OnPaste);
        this.EditCommand = this.CreateCommand(this.OnEdit);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
    }

    public ICommand CopyCommand { get; }

    public ICommand EditCommand { get; }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; set; }

    public ICommand PasteCommand { get; }

    public Project? Project {
        get => this._project;
        set => this.RaiseAndSetIfChanged(ref this._project, value);
    }

    private ObservableCollection<IExplorerItem> Items { get; } = [];

    public IExplorerItem? SelectedItem { get; set; }
    public ICommand DeleteCommand { get; }

    public ClientRootNode AddClientNode(ClientGroup cg, Type? type = null) {
        var c = Dispatcher.UIThread.Invoke(() => {
            var n = this.Items.FirstOrDefault(t => ((ClientRootNode)t).Client.Path == cg.Path);
            if (n == null) {
                var clientNode = new ClientRootNode(cg, type);
                clientNode.Init();
                this.Items.Add(clientNode);
                this.ExplorerTree.RowSelection!.Select(new IndexPath(0));
                return clientNode;
            }

            return (ClientRootNode)n;
        });
        return c;
    }

    public override void Dispose() {
        base.Dispose();
        var treeDataGridRowSelectionModel = this.ExplorerTree.RowSelection;
        if (treeDataGridRowSelectionModel != null) {
            treeDataGridRowSelectionModel.SelectionChanged -= this.RowSelectionChanged;
        }
    }

    public ClientRootNode[] GetClientNodes() =>
        this.Items.Where(c => c is ClientRootNode).Cast<ClientRootNode>().ToArray();

    public void LoadProject(string path) {
        if (string.IsNullOrWhiteSpace(path)) {
            return;
        }

        if (!Directory.Exists(path)) {
            return;
        }

        if (path == this.Project?.Path) {
            return;
        }

        var stateFile = Path.Combine(path, ProjectSaveState.FileName);
        if (!this.Io.File.Exists(stateFile)) {
            this.Io.Log.Error($"{path} is not a valid project path. Please select another folder");
            return;
        }

        //Close all existing tabs
        GlobalHub.publish(new CloseAllTabsMessage());

        var load = new LoadProjectFeature(this.Io, path);
        this.Project = load.Run();

        this.Items.Clear();
        foreach (var client in this.Project.Clients) {
            this.AddClientNode(client);
        }

        //if there are no clients, show the add dialog
        if (!this.Project.Clients.Any()) {
            var overlay = new AddGrpcServiceOverlayViewModel(this.Project);
            GlobalHub.publish(new OpenOverlayMessage(overlay));
        }
    }

    private void OnClientCompile(ClientCompileMessage message) => this.IsBusy = message.InProgress;

    private void OnClientDeleted(ClientDeletedMessage obj) {
        var target = this.Items.FirstOrDefault(t => t is ClientRootNode cn && cn.ClientPath == obj.Client.Path);
        if (target != null) {
            this.Items.Remove(target);
        }
    }

    private void OnDelete() {
        var header = "Please confirm";
        switch (this.SelectedItem) {
            case FileNode fn: {
                var dialog = new YesNoOverlayViewModel(header, $"Delete {fn.Title} file. Are you sure?", fn.DeleteCommand, EmptyCommand);
                GlobalHub.publish(new OpenOverlayMessage(dialog));
                break;
            }

            case MultiNodeFile mFilesNode: {
                var dialog = new YesNoOverlayViewModel(header, $"Delete {mFilesNode.Items.Count} files. Are you sure?", mFilesNode.DeleteCommand, EmptyCommand);
                GlobalHub.publish(new OpenOverlayMessage(dialog));
                break;
            }
        }
    }

    private void OnCopy() {
        foreach (var item in this.Items) {
            var selected = item.FindSelected();
            if (selected is FileNode fn) {
                this._copyPastePending = new CopyPasteArg(fn.Parent, fn.FullPath);
            }
        }
    }

    private void OnEdit() {
        foreach (var item in this.Items) {
            var selected = item.FindSelected();
            switch (selected) {
                case FileNode fn:
                    fn.IsEditing = true;
                    break;

                case ClientRootNode cn:
                    cn.OpenClientConfigCommand.Execute(Unit.Default);
                    break;
            }
        }
    }

    private void OnFileChanged(FileChangeMessage obj) => Dispatcher.UIThread.Invoke(() => this.OnFileChangedInternal(obj));

    private void OnFileChangedInternal(FileChangeMessage obj) {
        void Traverse(IExplorerItem[] items, FileChangeMessage msg, Action<IExplorerItem, FileChangeMessage> doAction, Func<IExplorerItem, bool> check) {
            lock (this) {
                var item = items.FirstOrDefault();
                if (item == null) {
                    return;
                }

                if (check(item)) {
                    doAction(item, msg);
                }
                else {
                    Traverse(item.Items.ToArray(), msg, doAction, check);
                }

                Traverse(items.Skip(1).ToArray(), msg, doAction, check);
            }
        }

        void Delete(IExplorerItem item, FileChangeMessage msg) {
            if (item is FileReqNode node && node.FullPath == msg.FullPath) {
                node.Parent?.Items.Remove(node);
            }
        }

        void FileChange(IExplorerItem item, FileChangeMessage msg) {
            if (item is FileNode node && node.FullPath == msg.FullPath) {
                node.CheckGitStatus();
            }
        }

        void Rename(IExplorerItem item, FileChangeMessage msg) {
            if (Path.GetExtension(msg.FullPath) != Ext.requestFileExt) {
                return;
            }

            var node = (MethodNode)item;
            var dir = Path.GetDirectoryName(msg.FullPath);
            var methodPath = ClientStructure.getMethodPath(node.Client.Path, node.MethodInfo.Name);
            if (dir == methodPath) {
                var existing = node.Items.Cast<FileReqNode>().FirstOrDefault(t => t.FullPath == msg.OldFullPath);
                if (existing != null) {
                    existing.UpdateFilePath(msg.FullPath);
                    existing.CheckGitStatus();
                }
                else {
                    var n = new FileReqNode(msg.FullPath);
                    node.AddItem(n);
                }
            }
        }

        void Create(IExplorerItem item, FileChangeMessage msg) {
            if (Path.GetExtension(msg.FullPath) != Ext.requestFileExt) {
                return;
            }

            var node = (MethodNode)item;
            var dir = Path.GetDirectoryName(msg.FullPath);
            var methodPath = ClientStructure.getMethodPath(node.Client.Path, node.MethodInfo.Name);
            if (dir == methodPath) {
                var existing = node.Items.Cast<FileReqNode>().FirstOrDefault(t => t.FullPath == msg.FullPath);
                if (existing == null) {
                    var n = new FileReqNode(msg.FullPath);
                    node.AddItem(n);
                }
            }
        }

        if (obj.ChangeType == WatcherChangeTypes.Deleted) {
            Traverse(this.Items.ToArray(), obj, Delete, i => i.Items.Count == 0);
        }

        if (obj.ChangeType == WatcherChangeTypes.Changed) {
            Traverse(this.Items.ToArray(), obj, FileChange, i => i.Items.Count == 0);
        }

        if (obj.ChangeType == WatcherChangeTypes.Renamed) {
            Traverse(this.Items.ToArray(), obj, Rename, i => i is MethodNode);
        }

        if (obj.ChangeType == WatcherChangeTypes.Created) {
            Traverse(this.Items.ToArray(), obj, Create, i => i is MethodNode);
        }
    }

    private void OnPaste() {
        if (this._copyPastePending == null) {
            return;
        }

        this.Exec(() => {
            foreach (var item in this.Items) {
                var selected = item.FindSelected();
                if (selected != null && (selected == this._copyPastePending.Container || selected.Parent == this._copyPastePending.Container)) {
                    var path = Path.GetDirectoryName(this._copyPastePending.FileToCopy);
                    if (path != null) {
                        var ext = Path.GetExtension(this._copyPastePending.FileToCopy);
                        var start = Path.GetFileNameWithoutExtension(this._copyPastePending.FileToCopy);
                        var fileCopy = Core.Utils.getAvailableFileName(path, start, ext);
                        fileCopy = Path.Combine(path, fileCopy);

                        this.Io.File.Copy(this._copyPastePending.FileToCopy, fileCopy);
                    }
                }
            }
        });
    }

    private async Task OnShowClient(ShowClientMessage obj) {
        async Task Foo() {
            var compileOutput = obj.Output;
            var types = ClientCompiler.getTypes(compileOutput.CompiledBytes);
            var clientTypes = ServiceClient.findClientType(types);
            var type = clientTypes.First(t => t.DeclaringType!.FullName.ToUpperInvariant() == obj.SelectedDiscoveredService.ToUpperInvariant());
            if (type != null && this.Project != null) {
                //Update the currently loaded project
                var feature = new AddClientFeature(this.Project,
                    obj.ClientName, obj.SelectedDiscoveredService!,
                    obj.ProtoFilesOrUrl, obj.Description, obj.CsFiles, obj.Dll,
                    //type,
                    this.Io);
                await feature.Add();

                //reload the project to take in the newly added client
                var loadProj = new LoadProjectFeature(this.Io, this.Project.Path);
                var proj = loadProj.Run();
                this.Project = proj;

                var client = proj.Clients.First(t => t.Name == obj.ClientName);
                this.AddClientNode(client, type);
            }
        }

        await this.Exec(Foo);
    }

    private void RowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) =>
        this.Exec(() => {
            foreach (var item in e.DeselectedItems.Where(i => i != null)) {
                item!.IsSelected = false;
            }

            this._nodeSelectionStrategy.Apply(e);

            var selectedNodes = this.GetClientNodes().SelectMany(c => c.FindChildNodes(d => d.IsSelected)).ToArray();
            if (selectedNodes.Length == 0) {
                this.SelectedItem = null;
            }
            else if (selectedNodes.Length == 1) {
                this.SelectedItem = selectedNodes[0];
            }
            else {
                var client = selectedNodes[0].FindParentNode<ClientRootNode>()!.Client;
                this.SelectedItem = new MultiNodeReqFiles(selectedNodes, client);
            }
        });

    private class CopyPasteArg(IExplorerItem? container, string fileToCopy) {
        public IExplorerItem? Container { get; } = container;
        public string FileToCopy { get; } = fileToCopy;
    }
}