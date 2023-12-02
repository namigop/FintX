#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ListTypeInfo : ITypeInfo {

    public ListTypeInfo(int index, Type itemType, ListNode parentNode) {
        this.Index = index;
        this.ItemType = itemType;
        this.ParentNode = parentNode;
    }

    public bool CanRead => true;
    public bool CanWrite => true;
    public FieldInfo FieldInfo => throw new NotImplementedException();
    public int Index { get; }
    public Type ItemType { get; }
    public string Name => "Item";
    public ListNode ParentNode { get; }
    public PropertyInfo PropertyInfo => throw new NotImplementedException(); //these are list items, not properties of class

    public object? GetValue(object parentInstance) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        return pi.GetValue(parentInstance, new object[] { this.Index });
    }

    public virtual void SetValue(object parentInstance, object value) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        pi.SetValue(parentInstance, value, new object[] { this.Index });
    }
}