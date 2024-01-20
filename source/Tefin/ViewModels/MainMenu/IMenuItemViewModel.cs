namespace Tefin.ViewModels.MainMenu;

public interface IMenuItemViewModel {
    bool IsSelected { get; set; }
    string Name { get; }
    string ShortName { get; }
    ISubMenusViewModel? SubMenus { get; }
}