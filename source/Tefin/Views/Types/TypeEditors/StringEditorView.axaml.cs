#region

using Avalonia.Controls;
using Avalonia.Input;

using Tefin.ViewModels.Types.TypeEditors;

#endregion

namespace Tefin.Views.Types.TypeEditors;

public partial class StringEditorView : UserControl {
    public StringEditorView() => this.InitializeComponent();
    //this.DataContextChanged += this.OnDataContextChanged;
    // private void OnDataContextChanged(object? sender, EventArgs e) {
    //     var c = (StringEditorView)sender!;
    //     if (c.DataContext != null)
    //         this.TextBox.Text = ((StringEditor)c.DataContext).TempValue;
    // }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
        //var tb = (TextBox)sender!;
        var vm = (StringEditor)this.DataContext!;

        if (e.Key == Key.Escape) {
            vm.Reset();
        }
        //
        // if (e.Key == Key.Enter) {
        //     vm.TempValue = tb.Text;
        //     this.MainBorder.Focus();
        // }
    }
}