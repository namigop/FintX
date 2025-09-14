#region

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ServiceMockMenuItemViewModel(MainMenuViewModel main) : MenuItemBaseViewModel(main), IMenuItemViewModel {
    //public ExplorerViewModel Explorer { get; set; } = new();
    public override string ToolTip { get; } = "Mock gRPC services";
    public override string Name { get; } = "gRPC Mocks";
    public override string ShortName { get; } = "server mocks";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    public void Init() {
    }
}