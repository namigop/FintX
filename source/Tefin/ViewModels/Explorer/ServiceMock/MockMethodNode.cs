using System.Reactive;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Tabs;
using Tefin.ViewModels.Tabs.Grpc;

using File = System.IO.File;

namespace Tefin.ViewModels.Explorer.ServiceMock;

public sealed class MockMethodNode : NodeBase {
    public MockMethodNode(MethodInfo methodInfo, ProjectTypes.ServiceMockGroup cg) {
        this.MethodInfo = methodInfo;
        this.ServiceMock = cg;
        this.CanOpen = true;
        this.Title = methodInfo.Name;
        this.SubTitle = $"{{{GrpcMethod.getMethodTypeFromClient(methodInfo)}}}";
        this.OpenMethodCommand = this.CreateCommand(this.OnOpenMethod);
        this.NewRequestCommand = this.CreateCommand(this.OnNewRequest);
        //this.ExportCommand = this.CreateCommand(this.OnExport);
        this.OpenMethodInWindowCommand = this.CreateCommand(this.OnOpenMethodInWindow);
        GlobalHub.subscribe<MessageProject.MsgServiceMockUpdated>(this.OnServiceMockUpdated).Then(this.MarkForCleanup);
    }

    public MethodInfo MethodInfo { get; }
    public ICommand NewRequestCommand { get; }

    public ICommand OpenMethodCommand { get; }

    //public ICommand ExportCommand { get; }
    public ICommand OpenMethodInWindowCommand { get; }

    public ProjectTypes.ServiceMockGroup ServiceMock { get; set; }

    public GrpcMockMethodHostViewModel CreateViewModel() => new(this.MethodInfo, this.ServiceMock);

    public override void Init() {
        ServiceMockStructure.getMethodPath(this.ServiceMock.Path, this.MethodInfo.Name)
            .Then(d => this.Io.Dir.CreateDirectory(d));

        var method = this.ServiceMock.Methods.FirstOrDefault(m => m.Name == this.MethodInfo.Name);
        if (method != null && File.Exists(method.ScriptFile)) {
            //foreach (var file in method.ScriptFile.OrderBy(f => f)) {
            var fn = new MockMethodScriptNode(method.ScriptFile);
            fn.Init();
            this.AddItem(fn);
            // }
        }
    }

    private void OnNewRequest() {
        var path = ClientStructure.getMethodPath(this.ServiceMock.Path, this.MethodInfo.Name);
        var file = Path.Combine(path, Core.Utils.getAvailableFileName(path, this.MethodInfo.Name, Ext.requestFileExt));

        var fn = new FileReqNode(file);
        this.AddItem(fn);
        this.IsExpanded = true;
        fn.OpenCommand.Execute(Unit.Default);
    }

    private void OnOpenMethod() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null) {
            GlobalHub.publish(new OpenTabMessage(tab));
        }
    }

    private void OnOpenMethodInWindow() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null) {
            GlobalHub.publish(new OpenChildWindowMessage(tab));
        }
    }

    private void OnServiceMockUpdated(MessageProject.MsgServiceMockUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.ServiceMock.Path == obj.Path || this.ServiceMock.Path == obj.PreviousPath) {
            this.ServiceMock = obj.Client;
            this.Io.Log.Debug($"Updated methodNode {this.MethodInfo.Name} clientInstance");
        }
    }
}