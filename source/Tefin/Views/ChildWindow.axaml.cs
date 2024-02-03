using Avalonia.Controls;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels;

namespace Tefin.Views;

public partial class ChildWindow : Window {
    public ChildWindow(ChildWindowViewModel vm) {
        this.DataContext = vm;
        vm.WindowClose = this.Close;
        this.InitializeComponent();
        this.Closed += this.OnWindowClosed;
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        var vm = (sender as ChildWindow)!.DataContext as ChildWindowViewModel;
        GlobalHub.publish(new CloseChildWindowMessage(vm.Content));
    }
}