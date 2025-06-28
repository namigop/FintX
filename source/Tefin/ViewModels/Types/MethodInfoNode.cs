#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Grpc;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Types;

public sealed class MethodInfoNode : TypeRootNode {
   
    public MethodInfoNode(MethodInfo mi, ProjectTypes.ClientGroup cg, List<VarDefinition> variables) : base(cg, variables){
        this.IsExpanded = true;
        this.CanOpen = true;
        this.Title = mi.Name;
        this.SubTitle = $"{{{GrpcMethod.getMethodType(mi)}}}";

    }
 
    public override void Init() {
    }
    
}