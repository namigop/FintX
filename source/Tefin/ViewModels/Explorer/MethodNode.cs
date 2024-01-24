#region

using System.Reactive;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Tabs;
using Tefin.ViewModels.Tabs.Grpc;

#endregion

namespace Tefin.ViewModels.Explorer;

public sealed class MethodNode : NodeBase {

    public MethodNode(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        this.MethodInfo = methodInfo;
        this.Client = cg;
        this.CanOpen = true;
        this.Title = methodInfo.Name;
        this.OpenMethodCommand = this.CreateCommand(this.OnOpenMethod);
        this.NewRequestCommand = this.CreateCommand(this.OnNewRequest);
    }

    public ProjectTypes.ClientGroup Client { get; set; }
    public MethodInfo MethodInfo { get; }
    public ICommand NewRequestCommand { get; }
    public ICommand OpenMethodCommand { get; }

    public ClientMethodViewModelBase CreateViewModel() {
        return new GrpcClientMethodHostViewModel(this.MethodInfo, this.Client);
    }

    public override void Init() {
        Project.getMethodPath(this.Client.Path, this.MethodInfo.Name)
           .Then(d => this.Io.Dir.CreateDirectory(d));

        var method = this.Client.Methods.FirstOrDefault(m => m.Name == this.MethodInfo.Name);
        if (method == null)
            return;

        foreach (var file in method.RequestFiles.OrderBy(f => f)) {
            var fn = new FileReqNode(file);
            fn.Init();
            this.AddItem(fn);
        }
    }

    private void OnNewRequest() {
        var path = Project.getMethodPath(this.Client.Path, this.MethodInfo.Name);
        var file = Path.Combine(path, Core.Utils.getAvailableFileName(path, this.MethodInfo.Name, Ext.requestFileExt));

        var fn = new FileReqNode(file);
        this.AddItem(fn);
        this.IsExpanded = true;
        fn.OpenCommand.Execute(Unit.Default);
    }

    private void OnOpenMethod() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null)
            GlobalHub.publish(new OpenTabMessage(tab));
    }
}