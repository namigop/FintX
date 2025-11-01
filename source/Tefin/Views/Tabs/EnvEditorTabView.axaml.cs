#region

using Avalonia.Controls;

using Tefin.Utils;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Views.Tabs;

public partial class EnvEditorTabView : UserControl {
    public EnvEditorTabView() {
        this.InitializeComponent();
        this.VarGrid.CellEditEnded += this.OnCellEditEnded;
        this.VarGrid.BeginningEdit += this.OnBeginningEdit;
        this.VarGrid.RowEditEnded += this.OnRowEditEnded;
    }

    private void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e) {
        var dg = sender as DataGrid;
        var thisVm = (EnvEditorTabViewModel)dg.FindParent<EnvEditorTabView>()!.DataContext!;
        thisVm.IsEditing = true;
    }

    private void OnCellEditEnded(object? sender, DataGridCellEditEndedEventArgs e) {
        if (e.Column.DisplayIndex == 0) {
            var rowVm = (e.Row.DataContext as EnvVarViewModel)!;
            rowVm.Name = rowVm.Name.Replace("{", "").Replace("}", "").Then(t => "{{" + t.ToUpperInvariant() + "}}");
        }
    }

    private void OnRowEditEnded(object? sender, DataGridRowEditEndedEventArgs e) {
        var dg = sender as DataGrid;
        var thisVm = (EnvEditorTabViewModel)dg.FindParent<EnvEditorTabView>()!.DataContext!;
        thisVm.IsEditing = false;
    }
}