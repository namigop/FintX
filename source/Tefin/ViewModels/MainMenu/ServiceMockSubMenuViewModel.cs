#region

using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Explorer.ServiceMock;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ServiceMockSubMenuViewModel : ViewModelBase, ISubMenusViewModel {
    private readonly ServiceMockExplorerViewModel _explorerViewModel;

    public ServiceMockSubMenuViewModel(ServiceMockExplorerViewModel explorerViewModel) {
        this._explorerViewModel = explorerViewModel;
        this.AddServiceMockCommand = this.CreateCommand(this.OnAddServiceMock);
    }

    public ICommand AddServiceMockCommand { get; }


    private void OnAddServiceMock() {
        AddGrpcMockOverlayViewModel overlayVm = new(this._explorerViewModel.Project!);
        OpenOverlayMessage msg = new(overlayVm);
        GlobalHub.publish(msg);
    }
}