#region

using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs;

public abstract class TabViewModelBase : ViewModelBase, ITabViewModel {
    private string _subTitle;
    private string _title;

    protected TabViewModelBase(IExplorerItem item) {
        this._title = "";
        this.Id = "";
        this.ExplorerItem = item;
        this._subTitle = item.SubTitle;
        item.Subscribe(nameof(item.Title), sender => this.Title = sender.Title)
            .Then(this.MarkForCleanup);
        this.CloseCommand = this.CreateCommand(this.OnClose);
        this.CloseAllCommand = this.CreateCommand(this.OnCloseAll);
        this.CloseAllOthersCommand = this.CreateCommand(this.OnCloseAllOthers);
        this.OpenInWindowCommand = this.CreateCommand(this.OnOpenInWindow);
        GlobalHub.subscribe<RemoveTreeItemMessage>(this.OnRemoveTreeItemRemoved).Then(this.MarkForCleanup);
    }

    public ICommand OpenInWindowCommand { get; }

    public virtual bool CanAutoSave { get; } = false;

    public ICommand CloseCommand { get; }
    public ICommand CloseAllCommand { get; }
    public ICommand CloseAllOthersCommand { get; }

    public IExplorerItem ExplorerItem { get; }

    public bool HasIcon => !string.IsNullOrEmpty(this.Icon);

    public abstract string Icon { get; }

    public string Id {
        get;
        protected set;
    }

    public string SubTitle {
        get => this._subTitle;
        set => this.RaiseAndSetIfChanged(ref this._subTitle, value);
    }

    public string Title {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public abstract void Init();

    private void OnOpenInWindow() {
        //close the tab but do not dispose it
        GlobalHub.publish(new RemoveTabMessage(this));
        GlobalHub.publish(new OpenChildWindowMessage(this));
    }

    private void OnCloseAll() => GlobalHub.publish(new CloseAllTabsMessage());

    private void OnCloseAllOthers() => GlobalHub.publish(new CloseAllOtherTabsMessage(this));

    protected virtual string GetTabId() => $"{this.Title}-{this.ExplorerItem.GetType().FullName}";

    protected virtual Task OnClose() {
        GlobalHub.publish(new CloseTabMessage(this));
        // if (this.ExplorerItem is IDisposable d) {
        //     d.Dispose();
        // }

        this.Dispose();

        return Task.CompletedTask;
    }

    private void OnRemoveTreeItemRemoved(RemoveTreeItemMessage obj) {
        if (this.ExplorerItem == obj.Item) {
            this.OnClose();
        }
    }
}