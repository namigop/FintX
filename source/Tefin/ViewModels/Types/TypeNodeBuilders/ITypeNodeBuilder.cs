namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public interface ITypeNodeBuilder {

    bool CanHandle(Type type);

    TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent);
}