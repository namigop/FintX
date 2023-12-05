#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class MetadataEntryTypeInfo : ITypeInfo {
    private readonly int _index;

    public MetadataEntryTypeInfo(int index, MetadataNode parentNode) {
        this._index = index;
        this.ParentNode = parentNode;
    }

    public bool CanRead => true;
    public bool CanWrite => true;
    public FieldInfo FieldInfo => throw new NotImplementedException();
    public string Name => "Item";
    public MetadataNode ParentNode { get; }

    public PropertyInfo PropertyInfo => throw new NotImplementedException(); //these are list items, not properties of class

    public object? GetValue(object parentInstance) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        return pi.GetValue(parentInstance, new object[] { this._index });
    }

    public void SetValue(object parentInstance, object value) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        pi.SetValue(parentInstance, value, new object[] { this._index });
    }
}