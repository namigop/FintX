#region

using System.Collections.ObjectModel;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;

using Tefin.Core.Infra.Actors;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;

using static Tefin.Core.Interop.ProjectTypes;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;

#endregion

namespace Tefin.ViewModels.Explorer;

public class ExplorerViewModel : ViewModelBase {
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
    }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; set; }
    public Project? Project { get; set; }
    private ObservableCollection<IExplorerItem> Items { get; } = new();

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
            cm.IsSelected = true;
            this.ExplorerTree.RowSelection!.Select(new IndexPath(0));
        }, DispatcherPriority.Input);

        GlobalHub.publish(new ExplorerUpdatedMessage());
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
}