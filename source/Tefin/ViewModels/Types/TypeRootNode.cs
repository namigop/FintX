using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Types;

 
public abstract class TypeRootNode : NodeBase{
    protected TypeRootNode(ProjectTypes.ClientGroup cg, List<VarDefinition> variables) {
        this.ClientGroup = cg;
        this.Variables = variables;
    }
 
    public List<VarDefinition> Variables { get; }
    public ProjectTypes.ClientGroup ClientGroup { get; }
}