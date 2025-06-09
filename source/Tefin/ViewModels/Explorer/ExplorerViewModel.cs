using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Overlay;

using static Tefin.Core.Interop.ProjectTypes;

namespace Tefin.ViewModels.Explorer;

public abstract class ExplorerViewModel<TRoot> : ViewModelBase, IExplorerTree<TRoot> where TRoot : ExplorerRootNode {
    private readonly IExplorerNodeSelectionStrategy _nodeSelectionStrategy;
    private CutCopyPasteArg? _copyPastePending;
    private Project? _project;

    protected ExplorerViewModel() {
        this._nodeSelectionStrategy = new SameNodeTypeStrategy<TRoot>(this);
        var temp = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new TemplateColumn<IExplorerItem>("", "CellTemplate", null, //edittemplate
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

        GlobalHub.subscribe<CutCopyNodeMessage>(this.OnCutCopyNodeMessage).Then(this.MarkForCleanup);
        GlobalHub.subscribe<PasteNodeMessage>(this.OnPasteNodeMessage).Then(this.MarkForCleanup);
        GlobalHub.subscribe<ClientDeletedMessage>(this.OnClientDeleted).Then(this.MarkForCleanup);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnMsgClientUpdated).Then(this.MarkForCleanup);
        GlobalHub.subscribe<ClientCompileMessage>(this.OnClientCompile).Then(this.MarkForCleanup);

        this.CopyCommand = this.CreateCommand(this.OnCopy);
        this.CutCommand = this.CreateCommand(this.OnCut);
        this.PasteCommand = this.CreateCommand(this.OnPaste);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
    }

    public ICommand DeleteCommand { get; }

    public ICommand PasteCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand CutCommand { get; }

    protected ObservableCollection<IExplorerItem> Items { get; } = [];

    public IExplorerItem? SelectedItem { get; private set; }
    protected abstract string[] SupportedExtensions { get; }

    public Project? Project {
        get => this._project;
        set => this.RaiseAndSetIfChanged(ref this._project, value);
    }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; set; }
    public void Clear() => this.Items.Clear();
    public TRoot[] GetRootNodes() => this.Items.Where(c => c is TRoot).Cast<TRoot>().ToArray();

    private void OnMsgClientUpdated(MessageProject.MsgClientUpdated obj) {
        void Load() {
            this.Exec(() => {
                var roots = this.GetRootNodes();
                if (roots.FirstOrDefault(t => t.ClientPath == obj.Client.Path) == null) {
                    var loadProj = new LoadProjectFeature(this.Io, this.Project!.Path);
                    var proj = loadProj.Run();
                    this.Project = proj;
                    this.AddRootNode(obj.Client);
                }
            });
        }

        Dispatcher.UIThread.Invoke(Load);
    }

    private void OnClientCompile(ClientCompileMessage message) => this.IsBusy = message.InProgress;

    private void OnPasteNodeMessage(PasteNodeMessage obj) {
        foreach (var r in this.GetRootNodes()) {
            if (r.FindChildNode(n => n == obj.Source) is not null) {
                this.OnPaste();
                break;
            }
        }
    }

    private void OnCutCopyNodeMessage(CutCopyNodeMessage obj) => this._copyPastePending = new CutCopyPasteArg(obj.PathsToCopy, obj.Nodes, obj.IsFile, obj.IsCut);

    private void OnCopy() => this.Exec(() => this._copyPastePending = this.CreateCutCopyArgs(false));

    protected abstract TRoot CreateRootNode(ClientGroup cg, Type? type = null);

    public TRoot AddRootNode(ClientGroup cg, Type? type = null) {
        var c = Dispatcher.UIThread.Invoke(() => {
            var n = this.Items.FirstOrDefault(t => ((TRoot)t).Client.Path == cg.Path);
            if (n == null) {
                var root = this.CreateRootNode(cg, type);
                root.Init();
                this.Items.Add(root);
                this.ExplorerTree.RowSelection!.Select(new IndexPath(0));
                return root;
            }

            return n;
        });

        return (TRoot)c;
    }

    private void OnClientDeleted(ClientDeletedMessage obj) {
        var target = this.Items.FirstOrDefault(t => t is TRoot cn && cn.ClientPath == obj.Client.Path);
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
            case FolderNode folderNode: {
                var dialog = new YesNoOverlayViewModel(header, $"Delete {folderNode.Title} folder. Are you sure?", folderNode.DeleteCommand, EmptyCommand);
                GlobalHub.publish(new OpenOverlayMessage(dialog));
                break;
            }
            case MultiNodeFile mFilesNode: {
                var dialog = new YesNoOverlayViewModel(header, $"Delete {mFilesNode.Items.Count} files. Are you sure?", mFilesNode.DeleteCommand, EmptyCommand);
                GlobalHub.publish(new OpenOverlayMessage(dialog));
                break;
            }
            // case MultiNodeFolder mFoldersNode: {
            //     var dialog = new YesNoOverlayViewModel(header, $"Delete {mFoldersNode.Items.Count} folders and its files. Are you sure?", mFoldersNode.DeleteCommand, EmptyCommand);
            //     GlobalHub.publish(new OpenOverlayMessage(dialog));
            //     break;
            // }
        }
    }

    private CutCopyPasteArg? CreateCutCopyArgs(bool isCutOperation) {
        foreach (var item in this.Items) {
            var nodes = item.FindChildNodes(i => i.IsSelected);
            foreach (var n in nodes) {
                n.IsCut = isCutOperation;
            }

            if (nodes.Count > 0) {
                var isFile = nodes[0] is FileNode;
                if (isFile) {
                    var files = nodes.Cast<FileNode>().Select(f => f.FullPath).ToArray();
                    return new CutCopyPasteArg(files, nodes.Cast<NodeBase>().ToArray(), true, isCutOperation);
                }
                else {
                    var files = nodes.Cast<FolderNode>().Select(f => f.FullPath).ToArray();
                    return new CutCopyPasteArg(files, nodes.Cast<NodeBase>().ToArray(), false, isCutOperation);
                }
            }
        }

        return null;
    }

    private void OnCut() => this.Exec(() => this._copyPastePending = this.CreateCutCopyArgs(true));

    private void RowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) =>
        this.Exec(() => {
            foreach (var item in e.DeselectedItems.Where(i => i != null)) {
                item!.IsSelected = false;
            }

            this._nodeSelectionStrategy.Apply(e);

            var selectedNodes = this.GetRootNodes().SelectMany(c => c.FindChildNodes(d => d.IsSelected)).ToArray();
            if (selectedNodes.Length == 0) {
                this.SelectedItem = null;
            }
            else if (selectedNodes.Length == 1) {
                this.SelectedItem = selectedNodes[0];
            }
            else {
                var client = selectedNodes[0].FindParentNode<TRoot>()!.Client;
                if (selectedNodes[0] is FileNode) {
                    this.SelectedItem = this.CreateMultiNodeFile(selectedNodes, client);
                }

                if (selectedNodes[0] is FolderNode) {
                    this.SelectedItem = this.CreateMultiNodeFolder(selectedNodes, client);
                }
            }
        });

    public override void Dispose() {
        base.Dispose();
        var treeDataGridRowSelectionModel = this.ExplorerTree.RowSelection;
        if (treeDataGridRowSelectionModel != null) {
            treeDataGridRowSelectionModel.SelectionChanged -= this.RowSelectionChanged;
        }
    }

    protected abstract MultiNodeFile CreateMultiNodeFile(IExplorerItem[] items, ClientGroup client);
    protected abstract NodeBase CreateMultiNodeFolder(IExplorerItem[] items, ClientGroup client);
    protected abstract string GetRootFilePath(string clientPath);

    private void OnPaste() {
        if (this._copyPastePending == null) {
            return;
        }

        static void PasteFileTo(IOs io, string path, CutCopyPasteArg arg) {
            var filesToCopy = arg.PathsToCopy;

            for (var i = 0; i < filesToCopy.Length; i++) {
                var fileToCopy = filesToCopy[i];
                if (string.IsNullOrWhiteSpace(fileToCopy) || !io.File.Exists(fileToCopy)) {
                    io.Log.Warn("Unable to copy. Source file is empty or does not exist");
                    continue;
                }

                if (arg.IsCut) {
                    var target = Path.Combine(path, Path.GetFileName(fileToCopy));
                    arg.Nodes[i].IsCut = fileToCopy != target;
                    io.File.Move(fileToCopy, target);
                }
                else {
                    var ext = Path.GetExtension(fileToCopy);
                    var start = Path.GetFileNameWithoutExtension(fileToCopy);
                    var fileCopy = Core.Utils.getAvailableFileName(path, start, ext);
                    fileCopy = Path.Combine(path, fileCopy);
                    io.File.Copy(fileToCopy, fileCopy);
                }
            }

            if (arg.IsCut) {
                arg.Clear();
            }
        }

        // static void PasteFolderTo(IOs io, string path, CutCopyPasteArg arg) {
        //     var foldersToCopy = arg.PathsToCopy;
        //
        //     for (var i = 0; i < foldersToCopy.Length; i++) {
        //         var folderToCopy = foldersToCopy[i];
        //         if (string.IsNullOrWhiteSpace(folderToCopy) || !io.Dir.Exists(folderToCopy)) {
        //             io.Log.Warn("Unable to copy. Source file is empty or does not exist");
        //             continue;
        //         }
        //
        //         var target = Path.Combine(path, Path.GetFileName(folderToCopy));
        //         if (io.Dir.Exists(path)) {
        //             if (arg.IsCut) {
        //                 arg.Nodes[i].IsCut = folderToCopy != target;
        //                 io.Dir.Move(folderToCopy, target);
        //             }
        //             else {
        //                 io.Dir.Copy(folderToCopy, target, true);
        //             }
        //         }
        //     }
        //
        //     if (arg.IsCut) {
        //         arg.Clear();
        //     }
        // }

        static void PasteTo(IOs io, string path, CutCopyPasteArg arg) {
            if (arg.IsFile) {
                PasteFileTo(io, path, arg);
            }
            // else {
            //     PasteFolderTo(io, path, arg);
            // }
        }

        this.Exec(() => {
            foreach (var item in this.Items) {
                var selected = item.FindSelected();
                if (selected is TRoot rootNode) {
                    //paste to the root node
                    var path = this.GetRootFilePath(rootNode.ClientPath);
                    PasteTo(this.Io, path, this._copyPastePending);
                    return;
                }

                if (selected is FileNode fn) {
                    // file node can be below the root node or a folder node
                    var path = "";
                    if (fn.Parent is FolderNode folder) {
                        path = folder.FullPath;
                    }
                    else if (fn.Parent is TRoot root) {
                        path = this.GetRootFilePath(root.ClientPath);
                    }
                    else {
                        //TODO: how to deal with files that has sub-files
                        Debugger.Break();
                    }

                    PasteTo(this.Io, path, this._copyPastePending);
                    return;
                }

                if (selected is FolderNode fn2) {
                    PasteTo(this.Io, fn2.FullPath, this._copyPastePending);
                }
            }
        });
    }

    private class CutCopyPasteArg(string[] pathsToCopy, NodeBase[] nodes, bool isFile, bool isCutOperation = false) {
        public NodeBase[] Nodes { get; private set; } = nodes;
        public bool IsFile { get; private set; } = isFile;
        public string[] PathsToCopy { get; private set; } = pathsToCopy;
        public bool IsCut { get; private set; } = isCutOperation;

        public void Clear() {
            this.IsFile = false;
            this.PathsToCopy = [];
            this.Nodes = [];
            this.IsCut = false;
        }
    }
}