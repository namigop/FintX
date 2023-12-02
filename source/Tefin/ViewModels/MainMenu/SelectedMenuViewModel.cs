#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class SelectedMenuViewModel : ViewModelBase {
    private IMenuItemViewModel? _menuItem;

    public IMenuItemViewModel? MenuItem {
        get => this._menuItem;
        set => this.RaiseAndSetIfChanged(ref this._menuItem, value);
    }
}