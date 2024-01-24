#region

using Grpc.Core;

using Tefin.ViewModels.Types.TypeNodeBuilders;

#endregion

namespace Tefin.ViewModels.Types;

public class MetadataNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent)
    : ListNode(name, type, propInfo, instance, parent) {

    public override string FormattedTypeName {
        get => $"{{{nameof(Metadata)}}}";
    }

    protected override TypeBaseNode CreateListItemNode(string name, Type itemType, Dictionary<string, int> processedTypeNames, int counter, object? current, TypeBaseNode? parent) {
        MetadataEntryTypeInfo? typeInfo = new(counter, this);
        return TypeNodeBuilder.Create(name, itemType, typeInfo, processedTypeNames, current, parent);
    }

    protected override Type GetItemType() {
        return typeof(Metadata.Entry);
        //return GetListType().GetGenericArguments().FirstOrDefault();
    }
}