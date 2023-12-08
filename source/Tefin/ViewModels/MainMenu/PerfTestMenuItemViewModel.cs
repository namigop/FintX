#region

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ConfigMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {

    public ConfigMenuItemViewModel(MainMenuViewModel main) : base(main) {
         this.Explorer = new ExplorerViewModel();
    }

    public ExplorerViewModel Explorer { get; set; }
    public override string Name { get; } = "Configuration";
    public override string ShortName { get; } = "config";
    public override ISubMenusViewModel? SubMenus { get; } = null;
    public override string ToolTip { get; } = "Edit app configuration";
}