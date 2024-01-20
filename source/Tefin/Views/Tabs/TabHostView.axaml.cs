#region

using System.Reactive;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Tefin.Utils;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Views.Tabs;

public partial class TabHostView : UserControl {

    public TabHostView() {
        this.InitializeComponent();
    }

    private void CancelButtonClick(object? sender, RoutedEventArgs e) {
        if (sender is StyledElement element) {
            //Search for the close button on the tab header
            var btn = element.FindParent<Button>(btn => btn.Flyout != null);
            if (btn != null) {
                btn.Flyout!.Hide();
            }
        }
    }

    private void CloseButtonClick(object? sender, RoutedEventArgs e) {
        if (sender is StyledElement element) {
            //Search for the close button on the tab header
            var btn = element.FindParent<Button>(btn => btn.Flyout != null);
            if (btn != null) {
                btn.Flyout!.Hide();
                var vm = (ITabViewModel)btn.DataContext!;
                vm.CloseCommand.Execute(Unit.Default);
            }
        }
    }
}