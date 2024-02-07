using Avalonia.Threading;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Tabs;
using Tefin.Views;

namespace Tefin.ViewModels;

public class HostWindowViewModel : ViewModelBase {
    public HostWindowViewModel() {
        GlobalHub.subscribeTask<OpenChildWindowMessage>(this.OnReceiveOpenChildWindowMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<CloseChildWindowMessage>(this.OnReceiveCloseChildWindowMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<FileChangeMessage>(this.OnReceiveFileChangeMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<ChildWindowClosedMessage>(this.OnReceiveChildWindowClosedMessage)
            .Then(this.MarkForCleanup);
    }

    public Dictionary<string, ChildWindowViewModel> Items { get; } = new();

    private void OnReceiveFileChangeMessage(FileChangeMessage msg) =>
        this.Exec(() => {
            var items = this.Items.Values.ToArray();
            if (msg.ChangeType == WatcherChangeTypes.Deleted) {
                var existingWindow =
                    items.FirstOrDefault(t => t.Content is PersistedTabViewModel && t.Content.Id == msg.FullPath);
                existingWindow?.Close();
            }

            if (msg.ChangeType == WatcherChangeTypes.Renamed) {
                var existingWindow = items.FirstOrDefault(t =>
                    t.Content is PersistedTabViewModel && t.Content.Id == msg.OldFullPath);
                if (existingWindow?.Content is PersistedTabViewModel pt) {
                    pt.UpdateTitle(msg.OldFullPath, msg.FullPath);
                }
            }
        });

    private void OnReceiveCloseChildWindowMessage(CloseChildWindowMessage obj) {
        if (this.Items.TryGetValue(obj.Content.Id, out var vm)) {
            vm.Close();
        }
    }

    private void OnReceiveChildWindowClosedMessage(ChildWindowClosedMessage obj) {
        if (this.Items.ContainsKey(obj.Content.Id)) {
            this.Items.Remove(obj.Content.Id);
        }
    }

    private async Task OnReceiveOpenChildWindowMessage(OpenChildWindowMessage obj) {
        obj.Content.Init();
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

        //Close any open tabs without disposing it
        GlobalHub.publish(new RemoveTabMessage(obj.Content));
    }
}