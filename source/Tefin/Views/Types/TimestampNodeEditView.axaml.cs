#region

using Avalonia.Controls;
using Avalonia.Input;

using Tefin.ViewModels.Types;

#endregion

namespace Tefin.Views.Types;

public partial class TimestampNodeEditView : UserControl {

    public TimestampNodeEditView() {
        this.InitializeComponent();
        this.TextBox.KeyDown += this.OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e) {
        var vm = (TimestampNode)this.DataContext;
        if (e.Key == Key.Enter) vm.CommitEdit();

        if (e.Key == Key.Escape) vm.Reset();
    }
}