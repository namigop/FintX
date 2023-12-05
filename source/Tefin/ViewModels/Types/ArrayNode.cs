#region

using System.Reflection;

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ArrayNode : ListNode {

    //private int listItemsCount;
    private readonly object _internalList;

    private readonly Type _internalListType;
    private readonly Type _itemType;
    private readonly ListTypeMethod _listMethods;

    public ArrayNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(name, type, propInfo, instance, parent) {
        this.ListItemsCount = 0;

        var listType = typeof(List<>);
        this._itemType = TypeHelper.getListItemType(this.Type).Value;
        var constructedListType = listType.MakeGenericType(this._itemType);

        this._listMethods = ListTypeMethod.GetMethods(constructedListType);
        var parentValue = parent != null ? Core.Utils.some(parent.Value)! : Core.Utils.none<object>();
        var (_, list) = TypeBuilder.getDefault(constructedListType, true, parentValue, 0);
        this._listMethods.ClearMethod!.Invoke(list, null);

        // var clearMethod = constructedListType.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
        // clearMethod!.Invoke(list, null);
        this._internalList = list;
        this._internalListType = this._internalList.GetType();
        if (instance != null) this._listMethods.AddRangeMethod.Invoke(this._internalList, new[] { instance });
        //var adr = this._internalListType.GetMethod("AddRange", BindingFlags.Public | BindingFlags.Instance);
        //adr.Invoke(this._internalList, new[] { instance });
        this.SubscribeTo(x => ((ArrayNode)x).ListItemsCount, this.OnCountChanged);
    }

    public override string FormattedTypeName => $"{{{this.Type.Name}}}";

    protected override Type GetItemType() {
        return this._itemType;
    }

    protected override object GetListInstance() {
        return this._internalList;
    }

    protected override Type GetListType() {
        return this._internalListType;
    }

    protected override ListTypeMethod GetMethods() {
        return this._listMethods;
    }

    private void OnCountChanged(ViewModelBase obj) {
        this.Value = this.ToArray(this._internalList);
    }

    private object? ToArray(object? targetList) {
        if (targetList == null)
            return null;

        var mi = targetList.GetType().GetMethod("ToArray", BindingFlags.Instance | BindingFlags.Public);
        var arrayInstance = mi!.Invoke(targetList, null);
        return arrayInstance;
    }
}