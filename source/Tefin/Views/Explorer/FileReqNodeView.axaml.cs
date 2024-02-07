#region

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.Views.Explorer;

public partial class FileReqNodeView : UserControl {
    public FileReqNodeView() {
        this.InitializeComponent();
        this.tbEditor.KeyDown += this.OnEditorKeyDown;
        this.tbEditor.LostFocus += this.OnEditorLostFocus;
        this.tbEditor.DoubleTapped += (sender, args) => args.Handled = true;
        //this.DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e) {
        var control = sender as FileReqNodeView;
        var vm = control!.DataContext as FileReqNode;
        vm?.SubscribeTo(x => ((FileReqNode)x).IsEditing,
            x => {
                if (((FileReqNode)x).IsEditing) {
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