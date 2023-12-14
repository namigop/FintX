#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ServerMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ServerMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.Explorer = new ExplorerViewModel();
    }

    public ExplorerViewModel Explorer { get; set; }
    public override string ToolTip { get; } = "host gRPC services";
    public override string Name { get; } = "Server Hosting";
    public override string ShortName { get; } = "server";
    public override ISubMenusViewModel? SubMenus { get; } = null;
}