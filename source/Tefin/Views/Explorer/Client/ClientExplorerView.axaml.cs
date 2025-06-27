#region

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Tabs;

#endregion

namespace Tefin.Views.Explorer.Client;

public partial class ClientExplorerView : UserControl {
    public ClientExplorerView() {
        this.InitializeComponent();
        this.TreeDg.DoubleTapped += this.OnDoubleTapped;
        //this.TreeDg.Tapped += this.TreeDgOnTapped;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e) {
        var c = e.Source as Control;
        var row = c.FindAncestorOfType<TreeDataGridRow>();
        if (row != null) {
            var context = row.DataContext;
            var item = (IExplorerItem)context!;
            if (item.CanOpen) {
                var vm = this.DataContext as ClientExplorerViewModel;
                var tab = TabFactory.From(item, vm!.Io);
                if (tab != null) {
                    GlobalHub.publish(new OpenTabMessage(tab));
                }
            }
        }
    }
}