#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class TimestampTypeInfo(int index, TimestampNode parentNode) : ITypeInfo {
    public TimestampNode ParentNode { get; } = parentNode;

    public bool CanRead => true;

    public bool CanWrite => true;

    public FieldInfo FieldInfo => throw new NotImplementedException();

    public string Name => "Item";

    public PropertyInfo PropertyInfo =>
        throw new NotImplementedException(); //these are list items, not properties of class

    public object? GetValue(object parentInstance) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        return pi?.GetValue(parentInstance, new object[] { index });
    }

    public void SetValue(object parentInstance, object? value) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        pi?.SetValue(parentInstance, value, new object[] { index });
    }
}