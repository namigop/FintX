#region

using System.Reactive;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.Views.Explorer;

public partial class ClientNodeActionView : UserControl {

    public ClientNodeActionView() {
        this.InitializeComponent();
    }

    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }

    private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
        var vm = (ClientNode)this.DataContext!;
        vm.DeleteCommand.Execute(Unit.Default);

        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }
}