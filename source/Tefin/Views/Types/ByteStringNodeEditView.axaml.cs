#region

using Avalonia.Controls;
using Avalonia.Input;

using Tefin.ViewModels.Types;

#endregion

namespace Tefin.Views.Types;

public partial class ByteStringNodeEditView : UserControl {
    public ByteStringNodeEditView() => this.InitializeComponent();

    private void OnKeyDownBase64TextBox(object? sender, KeyEventArgs e) {
        var vm = (ByteStringNode)this.DataContext!;
        if (e.Key == Key.Enter) {
            vm.CreateFromBase64String();
        }
    }
}