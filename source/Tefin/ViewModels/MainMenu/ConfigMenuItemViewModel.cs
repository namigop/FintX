#region

using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ConfigMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ConfigMenuItemViewModel(MainMenuViewModel main) : base(main) {
        //this.Explorer = new ExplorerViewModel();
        //this.SubMenus = new ConfigSubMenuViewModel(this.Explorer);
    }
    //public ExplorerViewModel Explorer { get; set; } = new();
    public override string ToolTip { get; } = "Edit app configuration";
    public override string Name { get; } = "Configuration";
    public override string ShortName { get; } = "config";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    public void Init() {
        //this.Explorer.Init();
        var envGroup = new EnvGroupNode() {Title = "Environments", SubTitle = "All environments"};
        var devEnv = new EnvNode() {Title = "DEV", SubTitle = "Development environments"};
        var uatEnv = new EnvNode() {Title = "DEV", SubTitle = "Development environments"};
        var prodEnv = new EnvNode() {Title = "DEV", SubTitle = "Development environments"};
        envGroup.AddItem(devEnv);
        envGroup.AddItem(uatEnv);
        envGroup.AddItem(prodEnv);
        //this.Explorer.Items.Add(envGroup);
    }
}