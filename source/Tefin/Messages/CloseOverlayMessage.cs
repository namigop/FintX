#region

using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.Messages;

public class CloseOverlayMessage(IOverlayViewModel overlayContent) : MessageBase {
    public IOverlayViewModel OverlayContent { get; } = overlayContent;
    public string Title { get; } = overlayContent.Title;
}