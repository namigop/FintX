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
    private readonly IDisposable _disposable;
    private string _subTitle;
    private string _title;

    protected TabViewModelBase(IExplorerItem item) {
        this.ExplorerItem = item;
        this._subTitle = item.SubTitle;
        this._title = item.Title;
        this._disposable = item.Subscribe(nameof(item.Title), sender => this.Title = sender.Title);

        this.CloseCommand = this.CreateCommand(this.OnClose);
        //this.Id = $"{item.Title}-{item.SubTitle}-{Guid.NewGuid()}";
        GlobalHub.subscribe<RemoveTreeItemMessage>(this.OnRemoveTreeItemRemoved);
    }

    public bool AllowDuplicates { get; set; } = false;
    public ICommand CloseCommand { get; }
    public IExplorerItem ExplorerItem { get; }
    public string Id {
        get => this.GetTabId();
    }

    public string SubTitle {
        get => this._subTitle;
        set => this.RaiseAndSetIfChanged(ref this._subTitle, value);
    }

    public string Title {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public virtual string GenerateNewTitle(string[] existingNames) {
        throw new Exception("Unable to generate a new tab title");
    }

    protected virtual string GetTabId() {
        return $"{this.Title}-{this.ExplorerItem.GetType().FullName}";
    }

    protected virtual Task OnClose() {
        GlobalHub.publish(new CloseTabMessage(this));
        if (this.ExplorerItem is IDisposable d) d.Dispose();
        this.Dispose();

        return Task.CompletedTask;
    }

    private void OnRemoveTreeItemRemoved(RemoveTreeItemMessage obj) {
        if (this.ExplorerItem == obj.Item) {
            this.OnClose();
        }
    }
}