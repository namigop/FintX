#region

using Tefin.Core.Reflection;

#endregion

namespace Tefin.ViewModels.Types.TypeNodeBuilders;

public class ListNodeBuilder : ITypeNodeBuilder {

    public bool CanHandle(Type type) {
        return TypeHelper.isGenericListType(type);
    }

    public TypeBaseNode Handle(string name, Type type, ITypeInfo propInfo, Dictionary<string, int> processedTypeNames, object? instance, TypeBaseNode? parent) {
        if (propInfo.CanWrite)
            return new ListNode(name, type, propInfo, instance, parent);

        return new ListNode(name, type, new ReadOnlyListTypeInfo(propInfo.PropertyInfo), instance, parent);
    }
}