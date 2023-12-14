#region

using System.Windows.Input;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.MainMenu;

public abstract class MenuItemBaseViewModel : ViewModelBase, IMenuItemViewModel {
    private readonly MainMenuViewModel _main;
    private bool _isSelected;

    protected MenuItemBaseViewModel(MainMenuViewModel main) {
        this._main = main;
        this.SelectItemCommand = this.CreateCommand(this.OnSelectItem);
    }
    public ICommand SelectItemCommand { get; }
    public abstract string ToolTip { get; }
    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }
    public abstract string Name { get; }
    public abstract string ShortName { get; }
    public abstract ISubMenusViewModel? SubMenus { get; }

    protected virtual void OnSelectItem() {
        this._main.SelectedMenuItem = this;
        //throw new System.NotImplementedException();
    }
}