#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Overlay;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class GrpcMockMethodHostViewModel : MockMethodViewModelBase {
    private string _importFile = "";

    public GrpcMockMethodHostViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi) {
        var type = GrpcMethod.getMethodTypeFromClient(mi);
        if (type == MethodType.Unary) {
            this.CallType = new MockUnaryViewModel(mi, cg);
        }
        else if (type == MethodType.ServerStreaming) {
            var str = "Mocking server streaming methods is not supported in the Community Edition.";
            var vm = new NotSupportedOverlayViewModel(str);
            OpenOverlayMessage msg = new(vm);
            GlobalHub.publish(msg);
            throw new NotSupportedException(str);
        }
        else if (type == MethodType.ClientStreaming) {
            var str = "Mocking client streaming methods is not supported in the Community Edition.";
            var vm = new NotSupportedOverlayViewModel(str);
            OpenOverlayMessage msg = new(vm);
            GlobalHub.publish(msg);
            throw new NotSupportedException(str);
        }
        else {
            var str = "Mocking duplex streaming methods is not supported in the Community Edition.";
            var vm = new NotSupportedOverlayViewModel(str);
            OpenOverlayMessage msg = new(vm);
            GlobalHub.publish(msg);
            throw new NotSupportedException(str);
        }

        this.CallType.SubscribeTo(x => x.IsBusy, vm => this.IsBusy = vm.IsBusy);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }

    public override string ApiType { get; } = GrpcPackage.packageName;
    public GrpMockCallTypeViewModelBase CallType { get; }
    public override bool IsLoaded => this.CallType.IsLoaded;

    public override void Dispose() {
        base.Dispose();
        this.CallType.Dispose();
    }

    public override string GetScriptContent() => this.CallType.GetScriptContent();

    public override void ImportScript(string scrptFile) => this._importFile = scrptFile;

    public void Init() =>
        this.Exec(() => {
            if (string.IsNullOrEmpty(this._importFile)) {
                this.CallType.Init();
                return;
            }

            if (!this.Io.File.Exists(this._importFile)) {
                this.CallType.Init();
                var content = this.GetScriptContent();
                this.Io.File.WriteAllText(this._importFile, content);
                return;
            }

            this.CallType.ImportScript(this._importFile);
        });

    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (!string.IsNullOrEmpty(this._importFile) && this._importFile.StartsWith(obj.PreviousPath)) {
            this._importFile = this._importFile.Replace(obj.PreviousPath, obj.Path);
        }
    }
}