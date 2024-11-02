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

    public virtual bool IsEditing {
        get => this._isEditing;
        set => this.RaiseAndSetIfChanged(ref this._isEditing, value);
    }

    public bool CanOpen {
        get;
        protected set;
    }

    public bool IsExpanded {
        get => this._isExpanded;
        set => this.RaiseAndSetIfChanged(ref this._isExpanded, value);
    }

    public bool IsSelected {
        get => this._isSelected;
        set {
            this.RaiseAndSetIfChanged(ref this._isSelected, value);
            if (!this._isSelected) {
                this.IsEditing = false; //cannot edit non-selected node
            }
        }
    }

    public ObservableCollection<IExplorerItem> Items { get; } = new();

    public IExplorerItem? Parent { get; set; }

    public string SubTitle {
        get => this._subTitle;
        set => this.RaiseAndSetIfChanged(ref this._subTitle, value);
    }

    public virtual string Title {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public IExplorerItem? FindSelected() => this.FindChildNode(i => i.IsSelected);

    public T? FindParentNode<T>(Func<IExplorerItem, bool>? predicate = null) where T : IExplorerItem {
        T? Find(IExplorerItem? item) {
            if (item == null) {
                return default;
            }

            if (item is T foundItem) {
                if (predicate != null) {
                    if (predicate.Invoke(item))
                        return foundItem;
                }
                else {
                    return foundItem;
                }
            }

            return Find(item.Parent);
        }

        return Find(this);
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

    public IExplorerItem? FindChildNode(Func<IExplorerItem, bool> predicate) {
        IExplorerItem? Find(ObservableCollection<IExplorerItem> items) {
            foreach (var item in items) {
                if (predicate(item)) {
                    return item;
                }

                var found = Find(item.Items);
                if (found != null) {
                    return found;
                }
            }

            return null;
        }

        if (predicate(this)) {
            return this;
        }

        return Find(this.Items);
    }

    public List<IExplorerItem> FindChildNodes(Func<IExplorerItem, bool> predicate) {
        List<IExplorerItem> Find(ObservableCollection<IExplorerItem> items, List<IExplorerItem> foundItems) {
            foreach (var item in items) {
                if (predicate(item)) {
                    foundItems.Add(item);
                }

                Find(item.Items, foundItems);
            }

            return foundItems;
        }

        var found = Find(this.Items, new List<IExplorerItem>());
        if (predicate(this)) {
            found.Add(this);
        }

        return found;
    }


    public abstract void Init();
}