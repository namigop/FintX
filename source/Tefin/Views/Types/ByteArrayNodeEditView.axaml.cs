#region

using Avalonia.Controls;
using Avalonia.Input;

using Tefin.ViewModels.Types;

#endregion

namespace Tefin.Views.Types;

public partial class ByteArrayNodeEditView : UserControl {
    public ByteArrayNodeEditView() => this.InitializeComponent();

    private void OnKeyDownBase64TextBox(object? sender, KeyEventArgs e) {
        var vm = (ByteArrayNode)this.DataContext!;
        if (e.Key == Key.Enter) {
            vm.CreateFromBase64String();
        }
    }
}