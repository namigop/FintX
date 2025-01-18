using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

using Tefin.Core.Interop;

namespace Tefin.ViewModels.Explorer.Config;

public class ConfigExplorerViewModel : ExplorerViewModel<ConfigGroupNode> {
    public ConfigExplorerViewModel() {
        this.SupportedExtensions = [".fxv"];
        //this._nodeSelectionStrategy = new FileOnlyStrategy(this);
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

        this.CopyCommand = this.CreateCommand(this.OnCopy);
        this.PasteCommand = this.CreateCommand(this.OnPaste);
        this.EditCommand = this.CreateCommand(this.OnEdit);
    }

    public ICommand CopyCommand { get; }

    public ICommand EditCommand { get; }

    public HierarchicalTreeDataGridSource<IExplorerItem> ExplorerTree { get; set; }

    public ICommand PasteCommand { get; }
    private ObservableCollection<IExplorerItem> Items { get; } = new();

    public IExplorerItem? SelectedItem { get; set; }
    protected override string[] SupportedExtensions { get; }

    private void RowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) =>
        this.Exec(() => {
            foreach (var item in e.DeselectedItems.Where(i => i != null)) {
                item!.IsSelected = false;
            }
        });

    protected override MultiNodeFile CreateMultiNodeFile(IExplorerItem[] items, ProjectTypes.ClientGroup client) {
        throw new NotImplementedException();
    }

    protected override NodeBase CreateMultiNodeFolder(IExplorerItem[] items, ProjectTypes.ClientGroup client) {
        throw new NotImplementedException();
    }

    protected override string GetRootFilePath(string clientPath) => throw new NotImplementedException();

    private void OnPaste() { }
    private void OnCopy() { }
    protected override ConfigGroupNode CreateRootNode(ProjectTypes.ClientGroup cg, Type? type = null) => throw new NotImplementedException();

    private void OnEdit() { }

    public void Init() {
        var envGroup = new ConfigGroupNode { Title = "Environments", SubTitle = "All environments" };
        var devEnv = new EnvNode { Title = "DEV", SubTitle = "Development environments" };
        var uatEnv = new EnvNode { Title = "UAT", SubTitle = "Development environments" };
        var prodEnv = new EnvNode { Title = "PROD", SubTitle = "Development environments" };
        envGroup.AddItem(devEnv);
        envGroup.AddItem(uatEnv);
        envGroup.AddItem(prodEnv);
        this.Items.Add(envGroup);
    }
}