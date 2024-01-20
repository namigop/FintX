#region

using System.Collections.ObjectModel;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Explorer;

public abstract class NodeBase : ViewModelBase, IExplorerItem {
    private bool _isEditing;
    private bool _isExpanded;
    private bool _isSelected;
    private string _subTitle = "";
    private string _title = "";

    public bool CanOpen {
        get;
        protected set;
    }

    public virtual bool IsEditing {
        get => this._isEditing;
        set => this.RaiseAndSetIfChanged(ref this._isEditing, value);
    }

    public bool IsExpanded {
        get => this._isExpanded;
        set => this.RaiseAndSetIfChanged(ref this._isExpanded, value);
    }

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }

    public ObservableCollection<IExplorerItem> Items {
        get;
    } = new();

    public IExplorerItem Parent { get; set; }

    public string SubTitle {
        get => this._subTitle;
        set => this.RaiseAndSetIfChanged(ref this._subTitle, value);
    }

    public virtual string Title {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public void AddItem(IExplorerItem child) {
        this.Items.Add(child);
        child.Parent = this;
    }

    public override void Dispose() {
        base.Dispose();
        foreach (var explorerItem in this.Items) {
            var n = (NodeBase)explorerItem;
            n.Dispose();
        }
    }

    public IExplorerItem FindSelected() {
        IExplorerItem Find(ObservableCollection<IExplorerItem> items) {
            foreach (var item in items) {
                if (item.IsSelected)
                    return item;
                var selected = Find(item.Items);
                if (selected != null)
                    return selected;
            }

            return null;
        }

        if (this.IsSelected)
            return this;

        return Find(this.Items);
    }

    public abstract void Init();
}