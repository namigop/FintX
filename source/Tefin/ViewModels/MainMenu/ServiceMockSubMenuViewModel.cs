#region

using System.Reactive;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Features;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ServiceMockSubMenuViewModel : ViewModelBase, ISubMenusViewModel {
    private readonly ClientExplorerViewModel _explorerViewModel;

    public ServiceMockSubMenuViewModel(ClientExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.AddServiceMockCommand = this.CreateCommand(this.OnAddServiceMock);
    }

    public ICommand AddServiceMockCommand { get; }
    

    private void OnAddServiceMock() {
        AddGrpcServiceOverlayViewModel overlayVm = new(this._explorerViewModel.Project!);
        OpenOverlayMessage msg = new(overlayVm);
        GlobalHub.publish(msg);
    }
}