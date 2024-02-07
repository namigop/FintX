#region

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ExceptionNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) => TypeHelper.isOfType_<Exception>(type);

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames,
        object? instance, TypeBaseNode? parent) {
        ExceptionNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}