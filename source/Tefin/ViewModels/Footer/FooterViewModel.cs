#region

using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;

#endregion

namespace Tefin.ViewModels.Footer;

public class FooterViewModel : ViewModelBase {
    private string _background;
    private string _message;

    public FooterViewModel() {
        this._background = Colors.info;
        this._message = "Ready...";
        GlobalHub.subscribe<Core.Interop.Messages.MsgShowFooter>(this.OnShowFooter);
    }

    public string Background {
        get => this._background;
        private set => this.RaiseAndSetIfChanged(ref this._background, value);
    }

    public string Message {
        get => this._message;
        private set => this.RaiseAndSetIfChanged(ref this._message, value);
    }

    private bool OnReset() {
        this.Background = "#2D3035";
        this.Message = "Ready...";
        return false; //Will stop the timer
    }

    private void OnShowFooter(Core.Interop.Messages.MsgShowFooter obj) {
        this.Background = obj.Color;
        this.Message = obj.Message;
        DispatcherTimer.Run(this.OnReset, TimeSpan.FromSeconds(30));
    }
}