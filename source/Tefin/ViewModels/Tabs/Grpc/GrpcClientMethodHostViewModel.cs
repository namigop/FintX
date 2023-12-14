#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class GrpcClientMethodHostViewModel : ClientMethodViewModelBase {
    public GrpcClientMethodHostViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi) {
        var type = GrpcMethod.getMethodType(mi);
        if (type == MethodType.Unary)
            this.CallType = new UnaryViewModel(mi, cg);
        else if (type == MethodType.ServerStreaming)
            this.CallType = new ServerStreamingViewModel(mi, cg);
        else if (type == MethodType.ClientStreaming)
            this.CallType = new ClientStreamingViewModel(mi, cg);
        else
            this.CallType = new DuplexStreamingViewModel(mi, cg);
        this.CallType.SubscribeTo(x => x.IsBusy, vm => this.IsBusy = vm.IsBusy);
        ;
    }

    public override string ApiType { get; } = GrpcPackage.packageName;

    public GrpCallTypeViewModelBase CallType { get; }

    public override void Dispose() {
        base.Dispose();
        this.CallType.Dispose();
    }

    public void Init() {
        this.CallType.Init();
    }
}