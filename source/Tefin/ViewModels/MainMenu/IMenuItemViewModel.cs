namespace Tefin.ViewModels.MainMenu;

public interface IMenuItemViewModel {
    string Name { get; }
    string ShortName { get; }

    bool IsSelected { get; set; }
    ISubMenusViewModel? SubMenus { get; }
}