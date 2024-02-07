#region

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ListNodeBuilder : ITypeNodeBuilder {
    public bool CanHandle(Type type) => TypeHelper.isGenericListType(type);

    public TypeBaseNode Handle(string name, Type type, ITypeInfo typeInfo, Dictionary<string, int> processedTypeNames,
        object? instance, TypeBaseNode? parent) {
        if (typeInfo.CanWrite) {
            return new ListNode(name, type, typeInfo, instance, parent);
        }

        return new ListNode(name, type, new ReadOnlyListTypeInfo(typeInfo.PropertyInfo!), instance, parent);
    }
}