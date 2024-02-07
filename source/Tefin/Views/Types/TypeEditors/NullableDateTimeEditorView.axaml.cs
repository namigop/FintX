#region

using Avalonia.Controls;

#endregion

namespace Tefin.Views.Types.TypeEditors;

public partial class NullableDateTimeEditorView : UserControl {
    public NullableDateTimeEditorView() => this.InitializeComponent();

    // private void OnKeyDown(object? sender, KeyEventArgs e) {
    //     var vm = (NullableDateTimeEditor)this.DataContext!;
    //     if (e.Key == Key.Enter) {
    //         vm.CommitEdit();
    //     }
    //
    //     if (e.Key == Key.Escape) {
    //         vm.Reset();
    //     }
    // }
}