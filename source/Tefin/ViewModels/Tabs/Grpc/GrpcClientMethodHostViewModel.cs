#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Grpc;
using Tefin.Utils;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class GrpcClientMethodHostViewModel : ClientMethodViewModelBase {
    private string _importFile = "";

    public GrpcClientMethodHostViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi) {
        var type = GrpcMethod.getMethodType(mi);
        if (type == MethodType.Unary) {
            this.CallType = new UnaryViewModel(mi, cg);
        }
        else if (type == MethodType.ServerStreaming) {
            this.CallType = new ServerStreamingViewModel(mi, cg);
        }
        else if (type == MethodType.ClientStreaming) {
            this.CallType = new ClientStreamingViewModel(mi, cg);
        }
        else {
            this.CallType = new DuplexStreamingViewModel(mi, cg);
        }

        this.CallType.SubscribeTo(x => x.IsBusy, vm => this.IsBusy = vm.IsBusy);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }

    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (!string.IsNullOrEmpty(this._importFile) && this._importFile.StartsWith(obj.PreviousPath)) {
            this._importFile = this._importFile.Replace(obj.PreviousPath, obj.Path);
        }
    }
    public override bool IsLoaded => this.CallType.IsLoaded;
    public override string ApiType { get; } = GrpcPackage.packageName;
    public GrpCallTypeViewModelBase CallType { get; }

    public override void Dispose() {
        base.Dispose();
        this.CallType.Dispose();
    }

    public override string GetRequestContent() => this.CallType.GetRequestContent();

    public override void ImportRequestFile(string requestFile) => this._importFile = requestFile;

    public void Init() =>
        this.Exec(() => {
           
            if (string.IsNullOrEmpty(this._importFile)) {
                this.CallType.Init();
                return;
            }

            if (!this.Io.File.Exists(this._importFile)) {
                this.CallType.Init();
                var content = this.GetRequestContent();
                this.Io.File.WriteAllText(this._importFile, content);
                return;
            }
            
            this.CallType.ImportRequest(this._importFile);
            
        });
}