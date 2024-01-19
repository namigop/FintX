#region

using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ClientSubMenuViewModel : ViewModelBase, ISubMenusViewModel {
    private readonly ExplorerViewModel _explorerViewModel;

    public ClientSubMenuViewModel(ExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.AddClientCommand = this.CreateCommand(this.OnAddClient);
        this.OpenFolderCommand = this.CreateCommand(this.OnOpenFolder);
    }

   
    public ICommand AddClientCommand { get; }
    public ICommand OpenFolderCommand { get; }

    private void OnAddClient() {
        AddGrpcServiceOverlayViewModel overlayVm = new(this._explorerViewModel.Project!);
        OpenOverlayMessage msg = new(overlayVm);
        GlobalHub.publish(msg);
    }
    private async Task OnOpenFolder() {
        var proj = await DialogUtils.SelectFolder();
        this._explorerViewModel.LoadProject(proj);
    }

}