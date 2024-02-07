#region

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Overlay;

public class OverlayHostViewModel : ViewModelBase {
    private bool _canShowOverlay;

    private IOverlayViewModel? _content;

    public OverlayHostViewModel() {
        GlobalHub.subscribe<OpenOverlayMessage>(this.OnOpenOverlay).Then(this.MarkForCleanup);
        GlobalHub.subscribe<CloseOverlayMessage>(this.OnCloseOverlay).Then(this.MarkForCleanup);
    }

    public bool CanShowOverlay {
        get => this._canShowOverlay;
        set => this.RaiseAndSetIfChanged(ref this._canShowOverlay, value);
    }

    public IOverlayViewModel? Content {
        get => this._content;
        set => this.RaiseAndSetIfChanged(ref this._content, value);
    }

    private void OnCloseOverlay(CloseOverlayMessage obj) {
        this.CanShowOverlay = false;
        this.Content = null;
    }

    private void OnOpenOverlay(OpenOverlayMessage obj) {
        this.Content = obj.OverlayContent;
        this.CanShowOverlay = true;
    }
}