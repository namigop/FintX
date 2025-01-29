#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ListTypeInfo(int index, Type itemType, ListNode parentNode) : ITypeInfo {
    public int Index { get; } = index;
    public Type ItemType { get; } = itemType;

    public ListNode ParentNode { get; } = parentNode;

    public bool CanRead => true;

    public bool CanWrite => true;

    public FieldInfo FieldInfo => throw new NotImplementedException();

    public string Name => "Item";

    public PropertyInfo PropertyInfo =>
        throw new NotImplementedException(); //these are list items, not properties of class

    public object? GetValue(object parentInstance) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        return pi?.GetValue(parentInstance, [this.Index]);
    }

    public virtual void SetValue(object parentInstance, object? value) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        pi?.SetValue(parentInstance, value, [this.Index]);
    }
}