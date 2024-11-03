#region

using System.Collections;
using System.Diagnostics;

using Microsoft.FSharp.Core;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types.TypeNodeBuilders;

#endregion

namespace Tefin.ViewModels.Types;

public class ListNode : TypeBaseNode {
    private readonly Type _itemType;
    private bool _isEditing;
    private int _listItemsCount;
    private int _targetListItemsCount;

    public ListNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent) : base(name,
        type, propInfo, instance, parent) {
        this.IsExpanded = true;
        this.SubscribeTo(x => ((ListNode)x).ListItemsCount, this.OnCountChanged);
        this._itemType = type.GetGenericArguments().FirstOrDefault()!;
    }

    public override string FormattedTypeName => $"{{{TypeHelper.getFormattedGenericName(this.Type)}}}";

    public override string FormattedValue => this.IsNull ? "null" : $"Count = {this.ListItemsCount}";

    public override bool IsEditing {
        get => this._isEditing;
        set {
            this._isEditing = value;
            if (!this._isEditing) {
                this.ListItemsCount = this.TargetListItemsCount;
            }
        }
    }

    public bool IsNullOrEmpty => this.IsNull || this.ListItemsCount == 0;

    public int ListItemsCount {
        get => this._listItemsCount;
        set {
            this.RaiseAndSetIfChanged(ref this._listItemsCount, value);
            this.RaisePropertyChanged(nameof(this.IsNullOrEmpty));
            if (this._listItemsCount > 0) {
                this.IsNull = false;
            }
        }
    }

    public int TargetListItemsCount {
        get => this._targetListItemsCount;
        set {
            Debug.WriteLine($"_targetListItemsCount = {value}");
            this.RaiseAndSetIfChanged(ref this._targetListItemsCount, value);
            //this.ListItemsCount = this.TargetListItemsCount;
        }
    }

    protected virtual string ItemName => "Item";

    public void AddItem(object itemInstance) {
        var listInstance = this.GetListInstance();
        this.GetMethods().AddMethod!.Invoke(listInstance, new[] { itemInstance });
        var itemType = this.GetItemType();
        var count = GetListSize(listInstance!);
        var name = $"{this.ItemName}[{count-1}]";
        var processedTypeNames = new Dictionary<string, int>();

        var node = this.CreateListItemNode(name, itemType, processedTypeNames, count, itemInstance, this);
        node.Init();
        node.IsExpanded = false;
        this.Items.Add(node);
        this._listItemsCount = this.Items.Count;
        this._targetListItemsCount = this.Items.Count;
        this.RaisePropertyChanged(nameof(this.FormattedValue));
    }

    public void RemoveItem(IExplorerItem item) {
        var index = this.Items.IndexOf(item);
        if (index >= 0) {
            this.Items.RemoveAt(index);
            dynamic listInstance = this.GetListInstance()!;
            listInstance?.RemoveAt(index);
            int count = 0;
            foreach (var i in Items) {
                var name = $"{this.ItemName}[{count}]";
                i.Title = name;
                count++;
            }
            
            this._listItemsCount = this.Items.Count;
            this._targetListItemsCount = this.Items.Count;
            this.RaisePropertyChanged(nameof(this.FormattedValue));
        }
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        var listInstance = this.GetListInstance();

        //if (this.GeneratePropertyNodes && TypeNode.CanCreateChildNodes(this.GetListType(), processedTypeNames, listInstance)) {
        if (DefaultNode.CanCreateChildNodes(this.GetListType(), processedTypeNames, listInstance)) {
            this.Items.Clear();
            var e = (IEnumerator)this.GetMethods().GetEnumeratorMethod!.Invoke(listInstance, null)!;
            var itemType = this.GetItemType();
            var counter = 0;

            while (e.MoveNext()) {
                var current = e.Current;
                var name = $"{this.ItemName}[{counter}]";
                var node = this.CreateListItemNode(name, itemType, processedTypeNames, counter, current, this);
                node.Init();
                this.Items.Add(node);

                counter++;
            }
        }

        this._listItemsCount = this.Items.Count;
        this._targetListItemsCount = this.Items.Count;
        this.IsExpanded = true;
        this.RaisePropertyChanged(nameof(this.IsNullOrEmpty));
    }

    protected static int GetListSize(object value) {
        var currentCount = -1;

        var countProp = value.GetType().GetProperty("Count");
        if (countProp != null) {
            currentCount = (int)countProp.GetValue(value)!;
        }

        if (currentCount == -1) {
            countProp = value.GetType().GetProperty("Length");
            if (countProp != null) {
                currentCount = (int)countProp.GetValue(value)!;
            }
        }

        if (currentCount == -1) {
            var asList = value as IList;
            currentCount = asList != null ? asList.Count : -1;
        }

        return currentCount;
    }

    protected virtual TypeBaseNode CreateListItemNode(string name, Type itemType,
        Dictionary<string, int> processedTypeNames, int counter, object? current, TypeBaseNode parent) {
        var inner = TypeNodeBuilder.Create(name, itemType, new ListTypeInfo(counter, itemType, this),
            processedTypeNames, current, parent);
        return inner;
    }

    protected virtual Type GetItemType() => this._itemType;

    protected virtual object? GetListInstance() => this.Value;

    protected virtual Type GetListType() => this.Type;

    protected virtual ListTypeMethod GetMethods() => ListTypeMethod.GetMethods(this.Type);

    protected override void OnValueChanged(object? oldValue, object? newValue) {
    }

    private void OnCountChanged(ViewModelBase obj) {
        void AddNodes(int targetCount, int currentCount, object listInstance, Type itemType) {
            var delta = targetCount - currentCount;
            for (var i = 0; i < delta; i++) {
                var counter = currentCount + i;
                var name = $"{this.ItemName}[{counter}]";
                Dictionary<string, int>? processedTypeNames = new();
                var (ok, instance) = TypeBuilder.getDefault(itemType, true, FSharpOption<object>.Some(listInstance), 0);
                if (ok) {
                    this.GetMethods().AddMethod!.Invoke(listInstance, new[] { instance });
                    var node = this.CreateListItemNode(name, itemType, processedTypeNames, counter, instance, this);
                    node.Init();
                    node.IsExpanded = false;
                    this.Items.Add(node);
                }
            }

            this.IsExpanded = true;
        }

        void RemoveNodes(int targetCount, int currentCount, object listInstance) {
            var delta = currentCount - targetCount;
            for (var i = 0; i < delta; i++) {
                var last = this.Items.Count - 1;
                this.Items.RemoveAt(last);
                this.GetMethods().RemoveAtMethod!.Invoke(listInstance, new object[] { last });
            }
        }

        var t = (ListNode)obj;
        var currentCount = t.Items.Count;
        var targetCount = t.ListItemsCount;

        if (targetCount == currentCount) {
            return;
        }

        var listInstance = this.GetListInstance();
        if (listInstance == null) {
            return;
        }

        if (targetCount > currentCount) {
            AddNodes(targetCount, currentCount, listInstance, this.GetItemType());
        }

        if (targetCount < currentCount) {
            RemoveNodes(targetCount, currentCount, listInstance);
        }
    }
}