#region

using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.Messages;

public class OpenOverlayMessage(IOverlayViewModel overlayContent) : MessageBase {
    public IOverlayViewModel OverlayContent { get; } = overlayContent;
    public string Title { get; } = overlayContent.Title;
}