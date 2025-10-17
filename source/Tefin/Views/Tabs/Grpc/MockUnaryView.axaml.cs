using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

using Tefin.ViewModels.Tabs.Grpc;

namespace Tefin.Views.Tabs.Grpc;

public partial class MockUnaryView : UserControl {
    public MockUnaryView() {
        InitializeComponent();
    }


    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button)
            return;

        var btn = (Button)sender;
        var src = btn.FindLogicalAncestorOfType<Button>();
        src?.Flyout?.Hide();
    }

    private void DeleteButtonClick(object? sender, RoutedEventArgs e) {
        if (sender is not Button)
            return;

        var btn = (Button)sender;
        var src = btn.FindLogicalAncestorOfType<Button>();
        if (src?.DataContext is ScriptViewModel vm) {
            vm.RemoveCommand.Execute(null);

            src.Flyout?.Hide();
        }
    }
}