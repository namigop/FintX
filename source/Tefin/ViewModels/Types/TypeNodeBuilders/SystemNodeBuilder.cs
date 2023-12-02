#region

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class SystemNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return SystemType.isSystemType(type);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object instance, TypeBaseNode parent) {
        SystemNode t = new(name, type, propInfo, instance, parent);
        return t;
    }
}