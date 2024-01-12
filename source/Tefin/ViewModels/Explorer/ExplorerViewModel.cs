#region

using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;

using static Tefin.Core.Interop.ProjectTypes;
using ClientCompiler = Tefin.Core.Build.ClientCompiler;
using Type = System.Type;

#endregion

namespace Tefin.ViewModels.Explorer;

public class ExplorerViewModel : ViewModelBase {
    private CopyPasteArg _copyPastePending;
    public ExplorerViewModel() {
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

        this.ExplorerTree.RowSelection.SelectionChanged += this.RowSelectionChanged;
        GlobalHub.subscribeTask<ShowClientMessage>(this.OnShowClient);
        GlobalHub.subscribe<ClientDeletedMessage>(this.OnClientDeleted);
        GlobalHub.subscribe<FileChangeMessage>(this.OnFileChanged);

        this.CopyCommand = this.CreateCommand(this.OnCopy);
        this.PasteCommand = this.CreateCommand(this.OnPaste);
        this.EditCommand = this.CreateCommand(this.OnEdit);
    }

    private void OnEdit() {
        foreach (var item in this.Items) {
            var selected = item.FindSelected();
            switch (selected) {
                case FileNode fn:
                    fn.IsEditing = true;
                    break;

                case ClientNode cn:
                    cn.OpenClientConfigCommand.Execute(Unit.Default);
                    break;
            }
        }
    }

    private void OnCopy() {
        foreach (var item in this.Items) {
            var selected = item.FindSelected();
            if (selected is FileNode fn) {
                var path = Path.GetDirectoryName(fn.FullPath);
                var ext = Path.GetExtension(fn.FullPath);
                var start = Path.GetFileNameWithoutExtension(fn.FullPath);
                var fileCopy = Core.Utils.getAvailableFileName(path, start, ext);
                fileCopy = Path.Combine(path, fileCopy);

                this._copyPastePending = new CopyPasteArg() {
                    Container = fn.Parent,
                    FileToCopy = fn.FullPath
                };
            }
        }
    }
    private void OnPaste() {
        if (this._copyPastePending == null)
            return;
        
        foreach (var item in this.Items) {
            var selected = item.FindSelected();
            if (selected != null && (selected == this._copyPastePending.Container || selected.Parent == this._copyPastePending.Container)) {
                var path = Path.GetDirectoryName(this._copyPastePending.FileToCopy);
                var ext = Path.GetExtension(this._copyPastePending.FileToCopy);
                var start = Path.GetFileNameWithoutExtension(this._copyPastePending.FileToCopy);
                var fileCopy = Core.Utils.getAvailableFileName(path, start, ext);
                fileCopy = Path.Combine(path, fileCopy);
                this.Io.File.Copy(this._copyPastePending.FileToCopy, fileCopy);
            }
        }
    }

    private void OnFileChanged(FileChangeMessage obj) {

        void Traverse(IExplorerItem[] items, FileChangeMessage msg, Action<IExplorerItem, FileChangeMessage> doAction, Func<IExplorerItem, bool> check) {
            lock (this) {
                var item = items.FirstOrDefault();
                if (item == null)
                    return;

                if (check(item))
                    doAction(item, msg);
                else
                    Traverse(item.Items.ToArray(), msg, doAction, check);

                Traverse(items.Skip(1).ToArray(), msg, doAction, check);
            }
        }
        
        void Delete(IExplorerItem item, FileChangeMessage msg) {
            if (item is FileReqNode node && node.FullPath == msg.FullPath) {
                node.Parent.Items.Remove(node);
            }
        }

        void Rename(IExplorerItem item, FileChangeMessage msg) {
            if (Path.GetExtension(msg.FullPath) != Ext.requestFileExt)
                return;

            var node = (MethodNode)item;
            var dir = Path.GetDirectoryName(msg.FullPath);
            var methodPath = Core.Project.getMethodPath(node.Client.Path, node.MethodInfo.Name);
            if (dir == methodPath) {
                var existing = node.Items.Cast<FileReqNode>().FirstOrDefault(t => t.FullPath == msg.OldFullPath);
                if (existing != null) {
                    existing.UpdateFilePath(msg.FullPath);
                }
                else {
                    var n = new FileReqNode(msg.FullPath);
                    node.AddItem(n);
                }
            }
        }

        void Create(IExplorerItem item, FileChangeMessage msg) {
            if (Path.GetExtension(msg.FullPath) != Ext.requestFileExt)
                return;

            var node = (MethodNode)item;
            var dir = Path.GetDirectoryName(msg.FullPath);
            var methodPath = Core.Project.getMethodPath(node.Client.Path,  node.MethodInfo.Name);
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

        if (obj.ChangeType == WatcherChangeTypes.Renamed) {
            Traverse(this.Items.ToArray(), obj, Rename, i => i is MethodNode);
        }

        if (obj.ChangeType == WatcherChangeTypes.Created) {
            Traverse(this.Items.ToArray(), obj, Create, i => i is MethodNode);
        }
    }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; set; }
    public ProjectTypes.Project? Project { get; set; }
    private ObservableCollection<IExplorerItem> Items { get; } = new();

    public ICommand CopyCommand { get; }
    public ICommand PasteCommand { get; }

    public ICommand EditCommand { get; }

    private void RowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) {
        foreach (var item in e.DeselectedItems) {
            item.IsSelected = false;
        }

        foreach (var item in e.SelectedItems) {
            item.IsSelected = true;
        }

    }

    private void OnClientDeleted(ClientDeletedMessage obj) {
        var target = this.Items.FirstOrDefault(t => t is ClientNode cn && cn.ClientPath == obj.Client.Path);
        if (target != null)
            this.Items.Remove(target);
    }

    public override void Dispose() {
        base.Dispose();
        var treeDataGridRowSelectionModel = this.ExplorerTree.RowSelection;
        if (treeDataGridRowSelectionModel != null)
            treeDataGridRowSelectionModel.SelectionChanged -= this.RowSelectionChanged;
    }

    public void AddClientNode(ClientGroup cg, Type? type = null) {
        var cm = new ClientNode(cg, type);

        Dispatcher.UIThread.Invoke(() => {
            cm.Init();
            this.Items.Add(cm);
            //cm.IsSelected = true;
            this.ExplorerTree.RowSelection!.Select(new IndexPath(0));
        }, DispatcherPriority.Input);

        GlobalHub.publish(new ExplorerUpdatedMessage());
    }

    public ClientNode[] GetClientNodes() {
        return this.Items.Where(c => c is ClientNode).Cast<ClientNode>().ToArray();
    }

    private async Task OnShowClient(ShowClientMessage obj) {
        var compileOutput = obj.Output;
        var types = ClientCompiler.getTypes(compileOutput.CompiledBytes);
        var type = ServiceClient.findClientType(types).Value;
        if (type != null && this.Project != null) {

            //Update the currently loaded project
            var feature = new AddClientFeature(this.Project, obj.ClientName, obj.SelectedDiscoveredService!, obj.ProtoFilesOrUrl, obj.Description, obj.CsFiles, this.Io);
            await feature.Add();

            //reload the project to take in the newly added client
            var proj = Core.Project.loadProject(this.Io, this.Project.Path);
            this.Project = proj;

            var client = proj.Clients.First(t => t.Name == obj.ClientName);
            this.AddClientNode(client, type);
        }
    }

    class CopyPasteArg {
        public string FileToCopy { get; set; }
        public IExplorerItem Container { get; set; }
    }
}