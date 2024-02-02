using System.Reactive;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Tefin.ViewModels.Explorer;

namespace Tefin.Views.Explorer;

public partial class ClientNodeContext : UserControl {
    public ClientNodeContext() {
        InitializeComponent();
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