using System.Reflection;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Utils;

namespace Tefin.ViewModels.Tabs.Grpc;

public abstract class GrpMockCallTypeViewModelBase : ViewModelBase {
    protected GrpMockCallTypeViewModelBase(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) {
        this.MethodInfo = mi;
        this.ServiceMock = cg;
        GlobalHub.subscribe<MessageProject.MsgServiceMockUpdated>(this.OnServiceMockUpdated).Then(this.MarkForCleanup);
    }

    public ProjectTypes.ServiceMockGroup ServiceMock { get; private set; }
    public MethodInfo MethodInfo { get; }
    public abstract bool IsLoaded { get; }
    public abstract string GetScriptContent();

    public abstract void Init();

    private void OnServiceMockUpdated(MessageProject.MsgServiceMockUpdated obj) {
        if (this.ServiceMock.Path == obj.Path || this.ServiceMock.Path == obj.PreviousPath) {
            this.ServiceMock = obj.Client;
        }
    }

    public abstract void ImportScript(string scriptFile);
}