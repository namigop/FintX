#region

using System.Collections.ObjectModel;

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Explorer;

public abstract class NodeBase : ViewModelBase, IExplorerItem {
    private bool _canOpen;
    private bool _isEditing;
    private bool _isExpanded;
    private bool _isSelected;
    private ObservableCollection<IExplorerItem> _items = new();
    private string _subTitle = "";
    private string _title = "";

    public bool CanOpen {
        get => this._canOpen;
        protected set => this._canOpen = value;
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
        get => this._items;
        private set => this._items = value;
    }

    public string SubTitle {
        get => this._subTitle;
        set => this.RaiseAndSetIfChanged(ref this._subTitle, value);
    }

    public string Title {
        get => this._title;
        set => this.RaiseAndSetIfChanged(ref this._title, value);
    }

    public override void Dispose() {
        base.Dispose();
        foreach (var explorerItem in this.Items) {
            var n = (NodeBase)explorerItem;
            n.Dispose();
        }
    }

    public abstract void Init();
}