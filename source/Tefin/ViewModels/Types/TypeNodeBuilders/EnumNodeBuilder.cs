using Tefin.Core.Reflection;

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class EnumNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return type.IsEnum || (TypeHelper.isNullable(type) && Nullable.GetUnderlyingType(type)!.IsEnum);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object instance, TypeBaseNode parent) {
        EnumNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}