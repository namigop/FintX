using System.Reactive;

using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer;

namespace Tefin.Views.Explorer;

public partial class FileReqNodeContext : UserControl {
    public FileReqNodeContext() => this.InitializeComponent();

    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }

    private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
        var vm = (FileReqNode)this.DataContext!;
        vm.DeleteCommand.Execute(Unit.Default);

        var fly = this.btnDelete.Flyout;
        fly?.Hide();
    }
}