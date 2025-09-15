#region

#endregion

using Tefin.ViewModels.Explorer.ServiceMock;

namespace Tefin.ViewModels.MainMenu;

public class ServiceMockMenuItemViewModel : MenuItemBaseViewModel, IMenuItemViewModel {
    public ServiceMockMenuItemViewModel(MainMenuViewModel main) : base(main) {
        this.Explorer = new ServiceMockExplorerViewModel();
        this.SubMenus = new ServiceMockSubMenuViewModel(this.Explorer);
    }

    public ServiceMockExplorerViewModel Explorer { get; }
    public override string ToolTip { get; } = "Mock gRPC services";
    public override string Name { get; } = "gRPC Mocks";
    public override string ShortName { get; } = "mocks";
    public override ISubMenusViewModel? SubMenus { get; } = null;

    public void Init() {
    }
}