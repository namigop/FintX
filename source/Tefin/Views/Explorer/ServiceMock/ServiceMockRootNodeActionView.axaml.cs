#region

using Avalonia.Controls;

#endregion

namespace Tefin.Views.Explorer.ServiceMock;

public partial class ServiceMockRootNodeActionView : UserControl {
    public ServiceMockRootNodeActionView() => this.InitializeComponent();

    // private void CancelButtonClick(object? sender, RoutedEventArgs e) {
    //     var fly = this.btnDelete.Flyout;
    //     fly?.Hide();
    // }
    //
    // private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
    //     var vm = (ServiceMockNode)this.DataContext!;
    //     vm.DeleteCommand.Execute(Unit.Default);
    //
    //     var fly = this.btnDelete.Flyout;
    //     fly?.Hide();
    // }
}