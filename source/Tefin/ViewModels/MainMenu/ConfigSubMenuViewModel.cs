using System.Windows.Input;

using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Config;

namespace Tefin.ViewModels.MainMenu;

public class ConfigSubMenuViewModel : ViewModelBase, ISubMenusViewModel {
    private readonly ConfigExplorerViewModel _explorerViewModel;
    public ConfigSubMenuViewModel(ConfigExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.AddEnvironmentCommand = this.CreateCommand(this.OnAddEnvironment);
        //this.ImportCommand = this.CreateCommand(this.OnImport);
    }
    public ICommand AddEnvironmentCommand { get; }
    private void OnAddEnvironment() {
        throw new NotImplementedException();
    }
}