using Tefin.Features;

namespace Tefin.ViewModels.Types;




public class ResponseStreamNode : ListNode {
    private readonly List<VarDefinition> _variables;
    private readonly string _clientPath;

    public ResponseStreamNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent, List<VarDefinition> variables, string clientPath)
        : base(name, type, propInfo, instance, parent) {
        this.IsExpanded = true;
        this._variables = variables;
        this._clientPath = clientPath;
    }
        

    public override string FormattedTypeName { get; } = "{async seq}";
    
    public List<VarDefinition> Variables => _variables;

    public string ClientPath => _clientPath;
    
     
}