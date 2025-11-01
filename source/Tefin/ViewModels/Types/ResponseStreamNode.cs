namespace Tefin.ViewModels.Types;

public class StreamNode : ListNode {
    public StreamNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent,
        List<VarDefinition> variables, string clientPath, bool isRequest)
        : base(name, type, propInfo, instance, parent) {
        this.IsExpanded = true;
        this.Variables = variables;
        this.ClientPath = clientPath;
        this.IsRequest = isRequest;
    }

    public string ClientPath { get; }

    public override string FormattedTypeName { get; } = "{async seq}";

    public bool IsRequest { get; }

    public List<VarDefinition> Variables { get; }
}