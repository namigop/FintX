namespace Tefin.ViewModels.Types;

public class ResponseNode : DefaultNode {

    public ResponseNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode parent) : base(name, type, propInfo, instance, parent) {
    }

    public override string FormattedTypeName { get; } = "";
}