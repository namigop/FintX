#region

using Avalonia.Controls;

using Tefin.ViewModels.MainMenu;

#endregion

namespace Tefin.Views.MainMenu;

public partial class MainMenuView : UserControl {
    public MainMenuView() => this.InitializeComponent();

    private void Init(object? sender, EventArgs e) {
        var vm = (MainMenuViewModel)this.DataContext!;
        vm.ClientMenuItem.SelectItemCommand.Execute(null);
    }
}