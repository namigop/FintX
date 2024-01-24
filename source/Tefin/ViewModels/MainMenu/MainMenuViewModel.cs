#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class MainMenuViewModel : ViewModelBase {
    private IMenuItemViewModel? _selectedMenuItem;

    public MainMenuViewModel() {
        this.ClientMenuItem = new ClientMenuItemViewModel(this);
        this.InfoMenuItem = new InfoMenuItemViewModel(this);
        this.ServerMenuItem = new ServerMenuItemViewModel(this);
        this.ConfigMenuItem = new ConfigMenuItemViewModel(this);
        this.SelectedMenu = new SelectedMenuViewModel();
    }

    public ClientMenuItemViewModel ClientMenuItem { get; }
    public ConfigMenuItemViewModel ConfigMenuItem { get; }
    public InfoMenuItemViewModel InfoMenuItem { get; }
    public SelectedMenuViewModel SelectedMenu { get; }

    public IMenuItemViewModel? SelectedMenuItem {
        get => this._selectedMenuItem;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedMenuItem, value);
            this.SelectedMenu.MenuItem = this._selectedMenuItem;

            foreach (var s in new IMenuItemViewModel[] {
                         this.ClientMenuItem, this.ServerMenuItem, this.ConfigMenuItem, this.InfoMenuItem
                     }) {
                s.IsSelected = s == this._selectedMenuItem;
            }
        }
    }

    public ServerMenuItemViewModel ServerMenuItem { get; }
}