#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class MetadataEntryTypeInfo(int index, MetadataNode parentNode) : ITypeInfo {
    public MetadataNode ParentNode { get; } = parentNode;

    public bool CanRead {
        get => true;
    }
    public bool CanWrite {
        get => true;
    }
    public FieldInfo FieldInfo {
        get => throw new NotImplementedException();
    }
    public string Name {
        get => "Item";
    }

    public PropertyInfo PropertyInfo {
        get => throw new NotImplementedException(); //these are list items, not properties of class
    }

    public object? GetValue(object parentInstance) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        return pi?.GetValue(parentInstance, new object[] {
            index
        });
    }

    public void SetValue(object parentInstance, object? value) {
        var pi = parentInstance.GetType().GetProperty(this.Name);
        pi?.SetValue(parentInstance, value, new object[] {
            index
        });
    }
}