namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ByteArrayNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) {
        return type.IsArray && type == typeof(byte[]);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        return new ByteArrayNode(name, type, propInfo, instance, parent);
    }
}