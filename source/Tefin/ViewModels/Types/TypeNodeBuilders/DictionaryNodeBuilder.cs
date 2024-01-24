#region

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class DictionaryNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return TypeHelper.isDictionaryType(type);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        DictionaryNode? node = new(name, type, propInfo, instance, parent);
        return node;
    }
}