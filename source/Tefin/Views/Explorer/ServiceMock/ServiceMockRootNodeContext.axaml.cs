using System.Reactive;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer.ServiceMock;

namespace Tefin.Views.Explorer.ServiceMock;

public partial class ServiceMockRootNodeContext : UserControl {
    public ServiceMockRootNodeContext() => this.InitializeComponent();

    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }

    private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
        var vm = (ServiceMockRootNode)this.DataContext!;
        vm.DeleteCommand.Execute(Unit.Default);

        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }
}