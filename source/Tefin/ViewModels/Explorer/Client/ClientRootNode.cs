using System.Windows.Input;

using Avalonia.Threading;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;

namespace Tefin.ViewModels.Explorer.Client;

public class ClientRootNode : RootNode {
    private bool _compileInProgress;
    private bool _sessionLoaded;
    
    public ClientRootNode(ProjectTypes.ClientGroup cg, Type? clientType) : base(cg, clientType) {
        this.Client = ProjectTypes.ClientGroup.Empty();
        this.CanOpen = true;
        this.ClientType = clientType;
        this.ClientName = "";
        this.ClientConfigFile = "";
        this.ClientPath = "";
        this.ServiceName = "";

        this.Update(cg);

        this.IsExpanded = true;
        this.CompileClientTypeCommand = this.CreateCommand(this.OnCompileClientType);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        //this.ImportCommand = this.CreateCommand(this.OnImport);
        this.ExportCommand = this.CreateCommand(this.OnExport);
        GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated)
            .Then(this.MarkForCleanup);
    }

    public ICommand ExportCommand { get; }
    
    public ICommand CompileClientTypeCommand { get; }

    public ICommand DeleteCommand { get; }

    public bool IsLoaded => this.Items.Count > 0 && this.Items[0] is not EmptyNode && this._sessionLoaded;

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipName = $"{this.ClientName}_export.zip";
        var zipFile = await share.GetZipFile(zipName);
        if (string.IsNullOrEmpty(zipFile)) {
            return;
        }

        var result = share.ShareClient(this.Io, zipFile, this.Client);
        if (result.IsOk) {
            this.Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            this.Io.Log.Error(result.ErrorValue);
        }
    }
    public override void Init() {
        if (this.ClientType == null) {
            return;
        }

        //if (this.Items.FirstOrDefault() is EmptyNode)
        foreach (var i in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(i));
        }

        this.Items.Clear();

        var methodInfos = ServiceClient.findMethods(this.ClientType);
        foreach (var m in methodInfos) {
            var item = new MethodNode(m, this.Client);
            item.Init();
            this.AddItem(item);
            Log.logInfo.Invoke($"Found method {item.Title}");
        }

        this.IsExpanded = true;

        var loadSessionFeature =
            new LoadSessionFeature(
                this.Client.Path,
                this.Items.Cast<MethodNode>(),
                this.Io,
                loaded => {
                    this._sessionLoaded = loaded;
                    this.RaisePropertyChanged(nameof(this.IsLoaded));
                });

        DispatcherTimer.RunOnce(loadSessionFeature.Run, TimeSpan.FromMilliseconds(100));
    }

    private void OnClientNameChanged() {
    }

    private void OnClientUpdated(MessageProject.MsgClientUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.ClientPath == obj.Path || this.ClientPath == obj.PreviousPath) {
            var cg = obj.Client;
            this.Update(cg);
        }
    }

    private async Task OnCompileClientType() {
        if (this._compileInProgress) {
            return;
        }

        try {
            this._compileInProgress = true;
            var protoFiles = Array.Empty<string>();
            var compile = new CompileFeature(this.ServiceName, this.ClientName, this.Desc, protoFiles, this.Url,
                this.Io);
            var csFiles = this.Client.CodeFiles;
            var (ok, compileOutput) = await compile.CompileExisting(csFiles);
            if (ok) {
                var types = ClientCompiler.getTypes(compileOutput.CompiledBytes);
                var clientTypes = ServiceClient.findClientType(types);
                if (clientTypes.Length == 0) {
                    throw new Exception("No client types found");
                }
                if (clientTypes.Length == 1) {
                    this.ClientType = clientTypes[0];
                }
                else {
                    this.ClientType = clientTypes.First(c => c.DeclaringType!.FullName!.ToUpperInvariant() == this.ServiceName.ToUpperInvariant());
                }

                this.Init();
            }
        }
        finally {
            this._compileInProgress = false;
        }
    }

    private void OnDelete() {
        var feature = new DeleteClientFeature(this.Client, this.Io);
        feature.Delete();

        foreach (var m in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(m));
        }

        this.Items.Clear();
        GlobalHub.publish(new ClientDeletedMessage(this.Client));
    }

    private void Update(ProjectTypes.ClientGroup cg) {
        this.Client = cg;
        this.CanOpen = true;
        this.ClientPath = cg.Path;
        this.ClientName = cg.Name;
        this.ServiceName = cg.Config.Value.ServiceName;
        this.Url = cg.Config.Value.Url;
        this.Title = cg.Config.Value.Name;
        this.SubTitle = cg.Config.Value.Description;
        this.Desc = cg.Config.Value.Description;
        this.ClientConfigFile = cg.ConfigFile;

        // foreach (var node in this.Items) {
        //     if (node is MethodNode methodNode) {
        //         methodNode.Client = cg;
        //     }
        // }
    }
}