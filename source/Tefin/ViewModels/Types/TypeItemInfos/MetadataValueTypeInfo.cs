using System.Reflection;

using Grpc.Core;

namespace Tefin.ViewModels.Types;

public class MetadataValueTypeInfo(MetadataEntryNode metadataEntryNode) : ITypeInfo {
    public bool CanRead { get; } = true;
    public bool CanWrite { get; } = true;
    public FieldInfo? FieldInfo    => throw new NotImplementedException();
    public string Name { get; } = "Key";
    public PropertyInfo? PropertyInfo => throw new NotImplementedException();

    public object? GetValue(object parent) {
        var pi = parent?.GetType().GetProperty(this.Name);
        return pi?.GetValue(parent);
    }

    public void SetValue(object parentInstance, object? value) {
        var newValue = value?.ToString() ?? "";
        
         
        var current = (Metadata.Entry)parentInstance;
        
        // parentNode.Metadata.GetValue().RemoveAt(parentNode.Index);
        // var val =
        var entry = new Metadata.Entry(current.Key, newValue);
        metadataEntryNode.Metadata.RemoveAt(metadataEntryNode.Index);
        metadataEntryNode.Metadata.Insert(metadataEntryNode.Index, entry);
        metadataEntryNode.Value = entry;
    }
}