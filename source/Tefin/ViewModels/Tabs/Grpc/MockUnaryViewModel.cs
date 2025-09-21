using System.Reflection;

using Tefin.Core.Interop;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg)
    {
    }

    public override bool IsLoaded { get; }
    public override string GetRequestContent() => throw new NotImplementedException();

    public override void Init() => throw new NotImplementedException();
}