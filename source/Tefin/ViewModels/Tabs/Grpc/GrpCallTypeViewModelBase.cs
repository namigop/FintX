#region

using System.Reflection;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Utils;

using static Tefin.Core.Interop.MessageProject;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public abstract class GrpCallTypeViewModelBase : ViewModelBase {
    protected GrpCallTypeViewModelBase(MethodInfo mi, ProjectTypes.ClientGroup cg) {
        this.MethodInfo = mi;
        this.Client = cg;
        GlobalHub.subscribe<MsgClientUpdated>(this.OnClientUpdated).Then(this.MarkForCleanup);
    }

    public ProjectTypes.ClientGroup Client { get; private set; }

    public MethodInfo MethodInfo { get; }
    public abstract bool IsLoaded { get; }
    public abstract string GetRequestContent();

    public abstract void ImportRequest(string requestFile);

    public abstract void Init();

    private void OnClientUpdated(MsgClientUpdated obj) {
        if (this.Client.Path == obj.Path || this.Client.Path == obj.PreviousPath) {
            this.Client = obj.Client;
        }
    }
}