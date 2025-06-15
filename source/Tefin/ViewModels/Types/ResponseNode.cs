namespace Tefin.ViewModels.Types;

public class ResponseNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent, List<RequestVariable> variables, string clientPath) 
    : DefaultNode(name, type, propInfo, instance, parent) {
    public override string FormattedTypeName { get; } = "";

    public List<RequestVariable> Variables => variables;

    public string ClientPath => clientPath;
}