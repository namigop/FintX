namespace Tefin.ViewModels.Types;

public class ResponseNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent)
    : DefaultNode(name, type, propInfo, instance, parent) {
    public override string FormattedTypeName { get; } = "";
}