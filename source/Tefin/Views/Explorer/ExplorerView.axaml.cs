#region

using System.ComponentModel;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Views.Explorer;

public partial class ExplorerView : UserControl {
    public ExplorerView() {
        this.InitializeComponent();
        this.TreeDg.DoubleTapped += this.OnDoubleTapped;
        this.TreeDg.Tapped += this.TreeDgOnTapped;
        this.TreeDg.SelectionChanging += this.TreeDgOnSelectionChanging;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e) {
        var c = e.Source as Control;
        var row = c.FindAncestorOfType<TreeDataGridRow>();
        if (row != null) {
            var context = row.DataContext;
            var item = (IExplorerItem)context!;
            if (item.CanOpen) {
                var vm = this.DataContext as ExplorerViewModel;
                var tab = TabFactory.From(item, vm!.Io);
                if (tab != null)
                    GlobalHub.publish(new OpenTabMessage(tab));
            }
        }
    }

    private void TreeDgOnSelectionChanging(object? sender, CancelEventArgs e) {
        var node = this.TreeDg.RowSelection?.SelectedItem;
        if (node is IExplorerItem item) {
            item.IsSelected = false;
        }
    }

    private void TreeDgOnTapped(object? sender, TappedEventArgs e) {
        var node = this.TreeDg.RowSelection?.SelectedItem;
        if (node is IExplorerItem item) {
            item.IsSelected = true;
        }
    }
}