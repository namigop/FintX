#region

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.ServiceMock;

#endregion

namespace Tefin.Views.Explorer.ServiceMock;

public partial class MockMethodScriptNodeView : UserControl {
    public MockMethodScriptNodeView() {
        this.InitializeComponent();
        this.tbEditor.KeyDown += this.OnEditorKeyDown;
        this.tbEditor.LostFocus += this.OnEditorLostFocus;
        this.tbEditor.DoubleTapped += (sender, args) => args.Handled = true;
        //this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e) {
        var control = sender as MockMethodScriptNodeView;
        var vm = control!.DataContext as MockMethodScriptNode;
        vm?.SubscribeTo(x => ((MockMethodScriptNode)x).IsEditing,
            x => {
                if (((MockMethodScriptNode)x).IsEditing) {
                    control.tbEditor.Focus();
                }
            });
    }

    private void OnEditorKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Enter) {
            var node = (sender as TextBox)!.DataContext as FileNode;
            node!.EndEdit();
        }

        if (e.Key == Key.Escape) {
            var node = (sender as TextBox)!.DataContext as FileNode;
            node!.CancelEdit();
        }
    }

    private void OnEditorLostFocus(object? sender, RoutedEventArgs e) {
        var node = (sender as TextBox)!.DataContext as FileNode;
        if (node!.IsEditing) {
            node.CancelEdit();
        }
    }
}