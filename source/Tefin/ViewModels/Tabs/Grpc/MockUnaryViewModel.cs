using System.Reflection;

using Tefin.Core.Interop;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg)
    {
    }

    public override bool IsLoaded { get; }
    public override string GetScriptContent() => "<script>";

    public override void Init() {}
}