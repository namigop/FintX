using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Types;

public abstract class TypeRootNode : NodeBase{
    protected TypeRootNode(ProjectTypes.ClientGroup cg, List<RequestVariable> variables) {
        this.ClientGroup = cg;
        this.Variables = variables;
    }
 
    public List<RequestVariable> Variables { get; }
    public ProjectTypes.ClientGroup ClientGroup { get; }
}