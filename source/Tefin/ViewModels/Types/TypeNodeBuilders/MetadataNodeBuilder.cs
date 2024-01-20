#region

using Grpc.Core;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class MetadataNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return type == typeof(Metadata);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        MetadataNode? t = new(name, type, propInfo, instance, parent);
        return t;
    }
}