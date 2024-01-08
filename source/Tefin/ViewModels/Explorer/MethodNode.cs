#region

using System.Linq;
using System.Reflection;

using Tefin.Core.Interop;
using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.ViewModels.Explorer;

public class MethodNode : NodeBase {
    public MethodInfo MethodInfo {
        get;
    }

    public MethodNode(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        //this.ClientVm = clientVm;
        this.MethodInfo = methodInfo;
        this.Client = cg;
        this.CanOpen = true;
        this.Title = methodInfo.Name;
    }

    public ProjectTypes.ClientGroup Client { get; set; }

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