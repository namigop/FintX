namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ArrayNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) => type.IsArray;

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames,
        object? instance, TypeBaseNode? parent) {
        ArrayNode? node = new(name, type, propInfo, instance, parent);
        return node;
    }
}