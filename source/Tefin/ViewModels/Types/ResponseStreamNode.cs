namespace Tefin.ViewModels.Types;

public class ResponseStreamNode : ListNode {

    public ResponseStreamNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent) : base(name, type, propInfo, instance, parent) {
        this.IsExpanded = true;
    }

    public override string FormattedTypeName { get; } = "{async seq}";
}