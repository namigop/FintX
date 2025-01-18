#region

using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Explorer.Config;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ConfigMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ConfigMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.Explorer = new ConfigExplorerViewModel();
        //this.SubMenus = new ConfigSubMenuViewModel(this.Explorer);
    }
    public ConfigExplorerViewModel Explorer { get; set; } = new();
    public override string ToolTip { get; } = "Edit app configuration";
    public override string Name { get; } = "Configuration";
    public override string ShortName { get; } = "config";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    public void Init() {
        this.Explorer.Init();
        //this.Explorer.Items.Add(envGroup);
    }
}