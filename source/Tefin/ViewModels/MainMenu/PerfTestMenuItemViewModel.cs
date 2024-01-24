#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ConfigMenuItemViewModel(MainMenuViewModel main) : MenuItemBaseViewModel(main), IMenuItemViewModel {
    public ExplorerViewModel Explorer { get; set; } = new();
    public override string Name { get; } = "Configuration";
    public override string ShortName { get; } = "config";
    public override ISubMenusViewModel? SubMenus { get; } = null;
    public override string ToolTip { get; } = "Edit app configuration";
}