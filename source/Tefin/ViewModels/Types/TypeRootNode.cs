using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Types;

public abstract class TypeRootNode : NodeBase {
    protected TypeRootNode(ProjectTypes.ClientGroup cg, List<VarDefinition> variables) {
        this.ClientGroup = cg;
        this.Variables = variables;
    }

    public ProjectTypes.ClientGroup ClientGroup { get; }

    public List<VarDefinition> Variables { get; }
}