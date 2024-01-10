#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ServerMenuItemViewModel(MainMenuViewModel main) : MenuItemBaseViewModel(main), IMenuItemViewModel {
    public ExplorerViewModel Explorer { get; set; } = new();
    public override string ToolTip { get; } = "host gRPC services";
    public override string Name { get; } = "Server Hosting";
    public override string ShortName { get; } = "server";
    public override ISubMenusViewModel? SubMenus { get; } = null;
}