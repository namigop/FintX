#region

using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer.Config;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ConfigMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ConfigMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.Explorer = new ConfigExplorerViewModel();
        //this.SubMenus = new ConfigSubMenuViewModel(this.Explorer);
    }
    public ConfigExplorerViewModel Explorer { get; } 
    public ProjectTypes.Project? Project {
        get => this.Explorer.Project;
        set => this.Explorer.Project = value;
    }

    public override string ToolTip { get; } = "Edit app variables";
    public override string Name { get; } = "Variables";
    public override string ShortName { get; } = "env";
    public override ISubMenusViewModel? SubMenus { get; } = null;
    public void Init(ProjectTypes.Project proj) {
        this.Project = proj;
        this.Explorer.Init();
    }
}