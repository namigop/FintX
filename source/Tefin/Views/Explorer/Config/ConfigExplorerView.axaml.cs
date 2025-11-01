using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Config;
using Tefin.ViewModels.Tabs;

namespace Tefin.Views.Explorer.Config;

public partial class ConfigExplorerView : UserControl {
    public ConfigExplorerView() {
        this.InitializeComponent();
        this.TreeDg.DoubleTapped += this.OnDoubleTapped;
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e) {
        var c = e.Source as Control;
        var row = c.FindAncestorOfType<TreeDataGridRow>();
        if (row != null) {
            var context = row.DataContext;
            var item = (IExplorerItem)context!;
            if (item.CanOpen) {
                var vm = this.DataContext as ConfigExplorerViewModel;
                var tab = TabFactory.From(item, vm!.Io);
                if (tab != null) {
                    GlobalHub.publish(new OpenTabMessage(tab));
                }
            }
        }
    }
}