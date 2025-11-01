using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin.ViewModels.Overlay;

public class NotSupportedOverlayViewModel : ViewModelBase, IOverlayViewModel {
    public NotSupportedOverlayViewModel(string message) {
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.Message = message;
    }

    public ICommand CancelCommand { get; }
    public string Message { get; }
    public ICommand OkayCommand { get; }
    public string Title { get; } = "Not supported";

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private void OnOkay() {
        Core.Utils.openBrowser("https://fintx.dev");
        this.Close();
    }
}