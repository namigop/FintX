using System.Collections.ObjectModel;
using System.Windows.Input;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;

namespace Tefin.ViewModels.Explorer;

public class ConfigExplorerViewModel : ViewModelBase {
    public ConfigExplorerViewModel() {
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

    private void RowSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<IExplorerItem> e) =>
        this.Exec(() => {
            foreach (var item in e.DeselectedItems.Where(i => i != null)) {
                item!.IsSelected = false;
            }
        });

    private void OnPaste() { }
    private void OnCopy() { }
    private void OnEdit() { }

    public void Init() {
        var envGroup = new EnvGroupNode { Title = "Environments", SubTitle = "All environments" };
        var devEnv = new EnvNode { Title = "DEV", SubTitle = "Development environments" };
        var uatEnv = new EnvNode { Title = "DEV", SubTitle = "Development environments" };
        var prodEnv = new EnvNode { Title = "DEV", SubTitle = "Development environments" };
        envGroup.AddItem(devEnv);
        envGroup.AddItem(uatEnv);
        envGroup.AddItem(prodEnv);
        this.Items.Add(envGroup);
    }
}