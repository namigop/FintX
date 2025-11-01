#region

using System.Collections.ObjectModel;
using System.Text;

using Avalonia.Threading;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Explorer;

public abstract class NodeBase : ViewModelBase, IExplorerItem {
    private bool _isCut;
    private bool _isEditing;
    private bool _isExpanded;
    private bool _isSelected;
    private string _subTitle = "";
    private string _title = "";

    public virtual bool IsEditing {
        get => this._isEditing;
        set => this.RaiseAndSetIfChanged(ref this._isEditing, value);
    }

    public virtual bool IsCut {
        get => this._isCut;
        set => this.RaiseAndSetIfChanged(ref this._isCut, value);
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

    public T? FindParentNode<T>(Func<T, bool>? predicate = null) where T : IExplorerItem {
        T? Find(IExplorerItem? item) {
            if (item == null) {
                return default;
            }

            if (item is T foundItem) {
                if (predicate != null) {
                    if (predicate.Invoke(foundItem)) {
                        return foundItem;
                    }
                }
                else {
                    return foundItem;
                }
            }

            return Find(item.Parent);
        }

        return Find(this);
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

        var found = Find(this.Items, []);
        if (predicate(this)) {
            found.Add(this);
        }

        return found;
    }

    public void AddItem(IExplorerItem child) {
        this.Items.Add(child);
        child.Parent = this;
    }

    public void Clear() {
        foreach (var item in this.Items.ToArray()) {
            this.RemoveItem((NodeBase)item);
        }
    }

    public override void Dispose() {
        base.Dispose();
        foreach (var explorerItem in this.Items) {
            var n = (NodeBase)explorerItem;
            n.Dispose();
        }
    }

    public string GetJsonPath() {
        var node = (IExplorerItem)this;
        var sb = new StringBuilder();

        List<string> parts = [];
        while (true) {
            if (node is null) {
                parts.Insert(0, "$");
                //sb.Insert(0, "$");
                break;
            }

            //sb.Insert(0, node.Title);
            //sb.Insert(0, '.');
            parts.Insert(0, node.Title);
            node = node.Parent;
        }

        parts.RemoveAt(1);
        return string.Join(".", parts);
        //return sb.ToString();
    }

    public abstract void Init();

    public void RemoveItem(NodeBase item) =>
        Dispatcher.UIThread.Invoke(() => {
            this.Items.Remove(item);
            item.Dispose();
        });
}