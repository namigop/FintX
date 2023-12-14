#region

using System.Reflection;

using Tefin.Core.Interop;
using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.ViewModels.Explorer;

public class MethodNode : NodeBase {
    private readonly MethodInfo _methodInfo;

    public MethodNode(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        //this.ClientVm = clientVm;
        this._methodInfo = methodInfo;
        this.Client = cg;
        this.CanOpen = true;
        this.Title = methodInfo.Name;
    }

    public ProjectTypes.ClientGroup Client { get; set; }

    public ClientMethodViewModelBase CreateViewModel() {
        return new GrpcClientMethodHostViewModel(this._methodInfo, this.Client);
    }

    public override void Init() {
    }
}