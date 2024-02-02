#region

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
using Tefin.ViewModels.Overlay;

using static Tefin.Core.Interop.MessageProject;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;

#endregion

namespace Tefin.ViewModels.Explorer;

public class ClientNode : NodeBase {
    private string _clientName = "";
    private Type? _clientType;
    private bool _compileInProgress;
    private string _desc = "";
    private bool _sessionLoaded;
    private string _url = "";

    public ClientNode(ProjectTypes.ClientGroup cg, Type? clientType) {
        this.Client = ProjectTypes.ClientGroup.Empty();
        this.CanOpen = true;
        this.ClientType = clientType;
        this.ClientName = "";
        this.ClientConfigFile = "";
        this.ClientPath = "";
        this.ServiceName = "";

        this.Update(cg);

        this.IsExpanded = true;
        this.AddItem(new EmptyNode());
        this.OpenClientConfigCommand = this.CreateCommand(this.OnOpenClientConfig);
        this.CompileClientTypeCommand = this.CreateCommand(this.OnCompileClientType);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        //this.ImportCommand = this.CreateCommand(this.OnImport);
        this.ExportCommand = this.CreateCommand(this.OnExport);
        GlobalHub.subscribe<MsgClientUpdated>(this.OnClientUpdated);
    }

    public ICommand ExportCommand { get; } 
    public ProjectTypes.ClientGroup Client {
        get;
        private set;
    }

    public string ClientConfigFile { get; private set; }

    public string ClientName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref this._clientName, value);
    }

    public string ClientPath { get; private set; }

    public Type? ClientType {
        get => this._clientType;
        set => this.RaiseAndSetIfChanged(ref this._clientType, value);
    }

    public ICommand CompileClientTypeCommand { get; }

    public ICommand DeleteCommand { get; }

    public string Desc {
        get => this._desc;
        set => this.RaiseAndSetIfChanged(ref this._desc, value);
    }

    public bool IsLoaded => this.Items.Count > 0 && this.Items[0] is not EmptyNode && this._sessionLoaded;

    //public ReadOnlyDictionary<string, string> Config { get; }
    public ICommand OpenClientConfigCommand { get; }

    public string ServiceName { get; private set; }

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipFile = await share.GetZipFile();
        var result = share.ShareClient(this.Io, zipFile, this.Client);
        if (result.IsOk) {
            Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            Io.Log.Error(result.ErrorValue);
        }
    }

    
    public void Clear() => this.Items.Clear();

    public override void Init() {
        if (this.ClientType == null) {
            return;
        }

        //if (this.Items.FirstOrDefault() is EmptyNode)
        foreach (var i in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(i));
        }

        this.Items.Clear();

        var methodInfos = ServiceClient.findMethods(this._clientType);
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

    private void OnClientUpdated(MsgClientUpdated obj) {
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
                this.ClientType = ServiceClient.findClientType(types).Value;
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

    private void OnOpenClientConfig() {
        var vm = new GrpcClientConfigViewModel(this.ClientConfigFile, this.OnClientNameChanged);
        GlobalHub.publish(new OpenOverlayMessage(vm));
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

        foreach (var node in this.Items) {
            if (node is MethodNode methodNode) {
                methodNode.Client = cg;
            }
        }
    }
}