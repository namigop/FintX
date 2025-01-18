#region

using Avalonia.Controls;

#endregion

namespace Tefin.Views.Explorer.Client;

public partial class ClientRootNodeActionView : UserControl {
    public ClientRootNodeActionView() => this.InitializeComponent();

    // private void CancelButtonClick(object? sender, RoutedEventArgs e) {
    //     var fly = this.btnDelete.Flyout;
    //     fly?.Hide();
    // }
    //
    // private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
    //     var vm = (ClientNode)this.DataContext!;
    //     vm.DeleteCommand.Execute(Unit.Default);
    //
    //     var fly = this.btnDelete.Flyout;
    //     fly?.Hide();
    // }
}