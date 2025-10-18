using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin.ViewModels.Overlay;

public class NotSupportedOverlayViewModel : ViewModelBase, IOverlayViewModel {
    public string Title { get; } = "Not supported";
    public string Message { get; } 
    public ICommand CancelCommand { get; }
    public ICommand OkayCommand { get; }

    public NotSupportedOverlayViewModel(string message) {
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.Message = message;
    }

    private void OnOkay() {
        Core.Utils.openBrowser("https://fintx.dev");
        this.Close();
    }

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));
}