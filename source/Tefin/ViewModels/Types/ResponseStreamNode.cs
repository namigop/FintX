using Tefin.Features;

namespace Tefin.ViewModels.Types;

public class StreamNode : ListNode {
    private readonly List<VarDefinition> _variables;
    private readonly string _clientPath;

    public StreamNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent, List<VarDefinition> variables, string clientPath, bool isRequest)
        : base(name, type, propInfo, instance, parent) {
        this.IsExpanded = true;
        this._variables = variables;
        this._clientPath = clientPath;
        this.IsRequest = isRequest;
    }
    
    public bool IsRequest { get; }

    public override string FormattedTypeName { get; } = "{async seq}";
    
    public List<VarDefinition> Variables => _variables;

    public string ClientPath => _clientPath;
    
     
}