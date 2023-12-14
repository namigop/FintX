#region

using Avalonia.Controls;
using Avalonia.Input;

using Tefin.ViewModels.Types.TypeEditors;

#endregion

namespace Tefin.Views.Types.TypeEditors;

public partial class DateTimeEditorView : UserControl {
    public DateTimeEditorView() {
        this.InitializeComponent();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        var vm = (DateTimeEditor)this.DataContext!;
        if (e.Key == Key.Enter) {
            vm.CommitEdit();
        }

        if (e.Key == Key.Escape) {
            vm.Reset();
        }
    }
}