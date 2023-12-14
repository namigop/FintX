#region

using Grpc.Core;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class MetadataEntryNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) {
        return type == typeof(Metadata.Entry);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        MetadataEntryNode? t = new(name, type, propInfo, instance, parent);
        return t;
    }
}