using System.Windows.Input;

using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels;

public class ChildWindowViewModel : ViewModelBase {
    private string _footerMessage;

    public ChildWindowViewModel(ITabViewModel content) {
        this.Content = content;

        this.DockCommand = this.CreateCommand(this.OnDock);
        this.FooterMessage = !string.IsNullOrEmpty(content.SubTitle) ? content.SubTitle : "Ready...";
    }

    public string FooterMessage {
        get => this._footerMessage;
        set => this.RaiseAndSetIfChanged(ref _footerMessage , value);
    }

    public Action WindowClose { get; set; }

    public ITabViewModel Content { get; }
    public ICommand DockCommand { get; }

    public void Close() {
        Dispatcher.UIThread.Invoke(() => this.WindowClose?.Invoke());
    }
    private void OnDock() {
        this.Close();
        GlobalHub.publish(new OpenTabMessage(this.Content));
    }
}