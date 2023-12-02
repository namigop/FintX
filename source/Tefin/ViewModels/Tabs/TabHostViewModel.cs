#region

using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TabHostViewModel : ViewModelBase {
    private ITabViewModel? _selectedItem;

    public TabHostViewModel() {
        GlobalHub.subscribe<OpenTabMessage>(this.OnReceiveTabOpenMessage);
        GlobalHub.subscribeTask<CloseTabMessage>(this.OnReceiveTabCloseMessage);
    }

    public ObservableCollection<ITabViewModel> Items { get; } = new();

    public ITabViewModel? SelectedItem {
        get => this._selectedItem;
        set => this.RaiseAndSetIfChanged(ref this._selectedItem, value);
    }

    private async Task OnReceiveTabCloseMessage(CloseTabMessage obj) {
        await Dispatcher.UIThread.InvokeAsync(() => {
            if (this.Items.Count > 1) {
                if (this.SelectedItem == obj.Tab) {
                    this.SelectedItem = this.Items.Last();
                }
            }
            
            this.Items.Remove(obj.Tab);
            
            
        });
    }

    private void OnReceiveTabOpenMessage(OpenTabMessage obj) {
        var existing = this.Items.FirstOrDefault(t => t.Id == obj.Tab.Id);
        if (existing != null) {
            if (obj.Tab.AllowDuplicates) {
                var existingNames = this.Items.Where(t => t.Title.StartsWith(obj.Tab.Title)).Select(t => t.Title).ToArray();
                string title = obj.Tab.GenerateNewTitle(existingNames);
                obj.Tab.Title = title;
                this.Items.Add(obj.Tab);
                this.SelectedItem = this.Items.Last();
            }
            else {
                this.SelectedItem = existing;
            }
        }
        else {
            this.Items.Add(obj.Tab);
            this.SelectedItem = this.Items.Last();
        }
    }
}