#region

using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.MainMenu;

public class ClientSubMenuViewModel : ViewModelBase, ISubMenusViewModel {

    public ClientSubMenuViewModel() {
        this.AddClientCommand = this.CreateCommand(this.OnAddClient);
    }

    public ICommand AddClientCommand { get; }

    private void OnAddClient() {
        AddGrpcServiceOverlayViewModel overlayVm = new();
        OpenOverlayMessage msg = new(overlayVm);
        GlobalHub.publish(msg);
    }
}