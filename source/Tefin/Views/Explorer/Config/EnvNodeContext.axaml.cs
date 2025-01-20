using System.Reactive;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer.Config;

namespace Tefin.Views.Explorer.Config;

public partial class EnvNodeContext : UserControl {
    public EnvNodeContext() => this.InitializeComponent();

    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }

    private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
        var vm = (EnvNode)this.DataContext!;
        vm.DeleteCommand.Execute(Unit.Default);

        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }
}