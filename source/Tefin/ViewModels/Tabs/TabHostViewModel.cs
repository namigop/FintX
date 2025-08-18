#region

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Windows.Input;

using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TabHostViewModel : ViewModelBase {
    private ITabViewModel? _selectedItem;

    public TabHostViewModel() {
        GlobalHub.subscribe<OpenTabMessage>(this.OnReceiveTabOpenMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribeTask<CloseTabMessage>(this.OnReceiveTabCloseMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<FileChangeMessage>(this.OnReceiveFileChangeMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<CloseAllTabsMessage>(this.OnReceiveCloseAllTabsMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribe<CloseAllOtherTabsMessage>(this.OnReceiveCloseAllOtherTabsMessage)
            .Then(this.MarkForCleanup);
        GlobalHub.subscribeTask<RemoveTabMessage>(this.OnReceiveRemoveTabMessage)
            .Then(this.MarkForCleanup);

        this.OpenWebSiteCommand = this.CreateCommand(() => {
            Core.Utils.openBrowser("https://fintx.dev");
            // var email = "erik.araojo@wcfstorm.com";
            // var subject = "Hey, can I try FintX Pro?";
            // var body = "I am interested in the Pro version.  Do you mind sending over the download link%3f";
            // var arg = $"mailto:{email}?Subject={subject}&Body={body}";
            // //var arg = $"mailto:{email}?subject=sdfsdf";
            // var pi = new ProcessStartInfo(arg) { UseShellExecute = true };
            // Process.Start(pi);
        });
    }

    public ICommand OpenWebSiteCommand { get; }

    public ObservableCollection<ITabViewModel> Items { get; } = new();

    public ITabViewModel? SelectedItem {
        get => this._selectedItem;
        set {
            if (this._selectedItem != null)
                this._selectedItem.IsSelected = false;
            if (value != null)
                value.IsSelected = true;
            
            this.RaiseAndSetIfChanged(ref this._selectedItem, value);
        }
    }

    private void OnReceiveCloseAllOtherTabsMessage(CloseAllOtherTabsMessage arg) {
        var tabs = this.Items.Where(t => t is PersistedTabViewModel).ToArray();
        foreach (var tab in tabs) {
            if (tab != arg.Tab) {
                tab.CloseCommand.Execute(Unit.Default);
                //await this.OnReceiveTabCloseMessage(new CloseTabMessage(tab));
            }
        }
    }

    private void OnReceiveCloseAllTabsMessage(CloseAllTabsMessage arg) {
        var tabs = this.Items.Where(t => t is PersistedTabViewModel).ToArray();
        foreach (var tab in tabs) {
            tab.CloseCommand.Execute(Unit.Default);
            //await this.OnReceiveTabCloseMessage(new CloseTabMessage(tab));
        }
    }

    private void OnReceiveFileChangeMessage(FileChangeMessage msg) {
        if (msg.ChangeType == WatcherChangeTypes.Deleted) {
            var existingTab = this.Items.FirstOrDefault(t => t is PersistedTabViewModel && t.Id == msg.FullPath);
            if (existingTab != null) {
                existingTab.CloseCommand.Execute(Unit.Default);
                //await this.OnReceiveTabCloseMessage(new CloseTabMessage(existingTab));
            }
        }

        if (msg.ChangeType == WatcherChangeTypes.Renamed) {
            var existingTab = this.Items.FirstOrDefault(t => t is PersistedTabViewModel && t.Id == msg.OldFullPath);
            if (existingTab is PersistedTabViewModel pt) {
                pt.UpdateTitle(msg.OldFullPath, msg.FullPath);
            }
        }
    }

    private async Task RemoveTab(ITabViewModel tab) =>
        await Dispatcher.UIThread.InvokeAsync(() => {
            if (this.Items.Count > 1) {
                if (this.SelectedItem?.Id == tab.Id) {
                    this.SelectedItem = this.Items.Last();
                }
            }

            var tab2 = this.Items.FirstOrDefault(t => t.Id == tab.Id);
            if (tab2 != null) {
                this.Items.Remove(tab2);
            }
        });

    private async Task OnReceiveRemoveTabMessage(RemoveTabMessage obj) => await this.RemoveTab(obj.Tab);

    private async Task OnReceiveTabCloseMessage(CloseTabMessage obj) => await this.RemoveTab(obj.Tab);

    // obj.Tab.CloseCommand.Execute(Unit.Default);
    private void OnReceiveTabOpenMessage(OpenTabMessage obj) {
        obj.Tab.Init();
        var existing = this.Items.FirstOrDefault(t => t.Id == obj.Tab.Id);
        if (existing != null) {
            this.SelectedItem = existing;
        }
        else {
            this.Items.Add(obj.Tab);
            this.SelectedItem = this.Items.Last();
        }

        //Close any open windows
        GlobalHub.publish(new CloseChildWindowMessage(obj.Tab));
    }
}