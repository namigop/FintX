using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels;
using Tefin.ViewModels.Tabs;

namespace Tefin.Views;

public partial class ChildWindow : Window {
    public ChildWindow(ChildWindowViewModel vm) {
        this.DataContext = vm;
        vm.WindowClose = this.Close;
        InitializeComponent();
        this.Closed += OnWindowClosed;
        
    }

    private void OnWindowClosed(object? sender, EventArgs e) {
        var vm = ((sender as ChildWindow)!).DataContext as ChildWindowViewModel;
        GlobalHub.publish(new CloseChildWindowMessage(vm.Content));
    }
}