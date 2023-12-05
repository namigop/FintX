#region

using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.ViewModels.Overlay;

using static Tefin.Core.Interop.Messages;

#endregion

namespace Tefin.ViewModels.Explorer;

public class ClientNode : NodeBase {
    private ProjectTypes.ClientGroup _client;
    private string _clientName ="";
    private Type? _clientType;
    private string _desc = "";
    private string _url = "";

    public ClientNode(ProjectTypes.ClientGroup cg, Type? clientType) {
        this._client = ProjectTypes.ClientGroup.Empty();
        this.CanOpen = true;
        this.ClientType = clientType;
        this.ClientName = "";
        this.ClientConfigFile = "";
        this.ClientPath = "";
        this.ServiceName = "";
        
        this.Update(cg);

        this.IsExpanded = true;
        this.Items.Add(new EmptyNode());
        this.OpenClientConfigCommand = CreateCommand(OnOpenClientConfig);
        this.CompileClientTypeCommand = CreateCommand(OnCompileClientType);
        this.DeleteCommand = CreateCommand(OnDelete);
        GlobalHub.subscribe<MsgClientUpdated>(OnClientUpdated);
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

    public string Desc {
        get => this._desc;
        set => this.RaiseAndSetIfChanged(ref this._desc, value);
    }

    //public ReadOnlyDictionary<string, string> Config { get; }
    public ICommand OpenClientConfigCommand { get; }

    public string ServiceName { get; private set; }

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }

    public ICommand DeleteCommand { get; }

    public bool IsLoaded {
        get => this.Items.Count > 0 && this.Items[0] is not EmptyNode;
    }

    public override void Init() {
        if (this.ClientType == null)
            return;

        //if (this.Items.FirstOrDefault() is EmptyNode) 
        foreach (var i in this.Items)
            GlobalHub.publish(new RemoveTreeItemMessage(i));

        this.Items.Clear();

        var methodInfos = ServiceClient.findMethods(this._clientType);
        foreach (var m in methodInfos) {
            //string clientPath, ProjectTypes.ClientConfig clientConfig
            var item = new MethodNode(m, this._client);
            this.Items.Add(item);
            Log.logInfo.Invoke($"Found method {item.Title}");
        }

        this.IsExpanded = true;
        this.RaisePropertyChanged(nameof(IsLoaded));
    }

    private void OnDelete() {
        var feature = new DeleteClientFeature(this._client, this.Io);
        feature.Delete();

        foreach (var m in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(m));
        }

        this.Items.Clear();
        GlobalHub.publish(new ClientDeletedMessage(this._client));
    }

    public void Clear() {
        this.Items.Clear();
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
        var protoFiles = Array.Empty<string>(); //TODO
        var compile = new CompileFeature(this.ServiceName, this.ClientName, this.Desc, protoFiles, this.Url, this.Io);
        var csFiles = this._client.CodeFiles;
        var (ok, compileOutput) = await compile.CompileExisting(csFiles);
        if (ok) {
            Type[]? types = Core.Build.ClientCompiler.getTypes(compileOutput.CompiledBytes);
            this.ClientType = ServiceClient.findClientType(types).Value;
            this.Init();
        }
    }

    private void OnOpenClientConfig() {
        var vm = new GrpcClientConfigViewModel(this.ClientConfigFile, this.OnClientNameChanged);
        GlobalHub.publish(new OpenOverlayMessage(vm));
    }

    private void Update(ProjectTypes.ClientGroup cg) {
        this._client = cg;
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