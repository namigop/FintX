#region

using System.Reflection;
using System.Windows.Input;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.ViewModels.Tabs;
using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.ViewModels.Explorer;

public class MethodNode : NodeBase {
    public MethodNode(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        //this.ClientVm = clientVm;
        this.MethodInfo = methodInfo;
        this.Client = cg;
        this.CanOpen = true;
        this.Title = methodInfo.Name;
        this.OpenMethodCommand = this.CreateCommand(this.OnOpenMethod);
    }

    public ICommand OpenMethodCommand {
        get;
    }

    public MethodInfo MethodInfo {
        get;
    }

    public ProjectTypes.ClientGroup Client { get; set; }

    private void OnOpenMethod() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null)
            GlobalHub.publish(new OpenTabMessage(tab));
    }

    public ClientMethodViewModelBase CreateViewModel() {
        return new GrpcClientMethodHostViewModel(this.MethodInfo, this.Client);
    }

    public override void Init() {
        var method = this.Client.Methods.FirstOrDefault(m => m.Name == this.MethodInfo.Name);
        if (method == null)
            return;

        foreach (var file in method.RequestFiles) {
            var fn = new FileReqNode(file);
            fn.Init();
            this.Items.Add(fn);
        }
        //load auto-saved files 
    }
}