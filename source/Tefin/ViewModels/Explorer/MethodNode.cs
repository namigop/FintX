#region

using System.Reactive;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
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
        this.ExportCommand = this.CreateCommand(this.OnExport);
        this.OpenMethodInWindowCommand = this.CreateCommand(this.OnOpenMethodInWindow);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }

    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.Client.Path == obj.Path || this.Client.Path == obj.PreviousPath) {
            this.Client = obj.Client;
            this.Io.Log.Debug($"Updated methodNode {this.MethodInfo.Name} clientInstance");
        }
    }

    public ProjectTypes.ClientGroup Client { get; set; }
    public MethodInfo MethodInfo { get; }
    public ICommand NewRequestCommand { get; }
    public ICommand OpenMethodCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand OpenMethodInWindowCommand { get; }

    private void OnOpenMethodInWindow() {
        var tab = TabFactory.From(this, this.Io);
        if (tab != null) {
            GlobalHub.publish(new OpenChildWindowMessage(tab));
        }
    }

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipName = $"{this.MethodInfo.Name}_export.zip";
        var zipFile = await share.GetZipFile(zipName);
        if (string.IsNullOrEmpty(zipFile)) {
            return;
        }

        var result = share.ShareMethod(this.Io, zipFile, this.MethodInfo.Name, this.Client);
        if (result.IsOk) {
            this.Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            this.Io.Log.Error(result.ErrorValue);
        }
    }

    public ClientMethodViewModelBase CreateViewModel() =>
        new GrpcClientMethodHostViewModel(this.MethodInfo, this.Client);

    public override void Init() {
        ClientStructure.getMethodPath(this.Client.Path, this.MethodInfo.Name)
            .Then(d => this.Io.Dir.CreateDirectory(d));

        var method = this.Client.Methods.FirstOrDefault(m => m.Name == this.MethodInfo.Name);
        if (method == null) {
            return;
        }

        foreach (var file in method.RequestFiles.OrderBy(f => f)) {
            var fn = new FileReqNode(file);
            fn.Init();
            this.AddItem(fn);
        }
    }

    private void OnNewRequest() {
        var path = ClientStructure.getMethodPath(this.Client.Path, this.MethodInfo.Name);
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
}