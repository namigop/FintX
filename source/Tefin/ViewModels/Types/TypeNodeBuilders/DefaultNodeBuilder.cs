namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class DefaultNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) => true; //default handling for all types

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames,
        object? instance, TypeBaseNode? parent) {
        DefaultNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}