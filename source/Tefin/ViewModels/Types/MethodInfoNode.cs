#region

using System.Reflection;

using Tefin.Grpc;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Types;

public sealed class MethodInfoNode : NodeBase {
    public MethodInfoNode(MethodInfo mi) {
        this.IsExpanded = true;
        this.CanOpen = true;
        this.Title = mi.Name;
        this.SubTitle = $"{{{GrpcMethod.getMethodType(mi)}}}";
    }

    public List<RequestVariable> Variables { get; } = [];
    
    public override void Init() {
    }
}