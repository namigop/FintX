#region

using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.Messages;

public class OpenOverlayMessage : MessageBase {
    public OpenOverlayMessage(IOverlayViewModel overlayContent) {
        this.Title = overlayContent.Title;
        this.OverlayContent = overlayContent;
    }

    public IOverlayViewModel OverlayContent { get; }
    public string Title { get; }
}