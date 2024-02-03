using Avalonia.Threading;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Views;

namespace Tefin.ViewModels;

public class HostWindowViewModel : ViewModelBase {
    public HostWindowViewModel() {
        GlobalHub.subscribeTask<OpenChildWindowMessage>(this.OnReceiveOpenChildWindowMessage);
        GlobalHub.subscribe<CloseChildWindowMessage>(this.OnReceiveCloseChildWindowMessage);
    }

    public Dictionary<string, ChildWindowViewModel> Items { get; } = new();

    private void OnReceiveCloseChildWindowMessage(CloseChildWindowMessage obj) {
        if (this.Items.ContainsKey(obj.Content.Id)) {
            this.Items.Remove(obj.Content.Id);
        }
    }

    private async Task OnReceiveOpenChildWindowMessage(OpenChildWindowMessage obj) =>
        await Dispatcher.UIThread.InvokeAsync(() => {
            if (this.Items.ContainsKey(obj.Content.Id)) {
                //bring to front
            }
            else {
                var vm = new ChildWindowViewModel(obj.Content);
                this.Items[obj.Content.Id] = vm;
                var childWindow = new ChildWindow(vm);
                childWindow.ShowActivated = true;
                childWindow.ShowInTaskbar = true;
                childWindow.Show();
            }
        });
}