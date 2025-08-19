using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin.ViewModels.Overlay;

public class DialogViewModel(string title, DialogType dialogType) : ViewModelBase, IOverlayViewModel {
    public string DialogIcon {
        get {
            switch (dialogType) {
                case DialogType.Error: return "Icon.Error32";
                case DialogType.Question: return "Icon.Question32";
                case DialogType.Warning: return "Icon.Warn32";
                default: return "Icon.Info32";
            }
        }
    }

    public string Title { get; } = title;

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));
}