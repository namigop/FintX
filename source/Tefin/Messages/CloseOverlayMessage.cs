#region

using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.Messages;

public class CloseOverlayMessage : MessageBase {
    public CloseOverlayMessage(IOverlayViewModel overlayContent) {
        this.Title = overlayContent.Title;
        this.OverlayContent = overlayContent;
    }

    public IOverlayViewModel OverlayContent { get; }
    public string Title { get; }
}