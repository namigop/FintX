using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Overlay;

namespace Tefin.ViewModels.Explorer.Client;

public abstract class RootNode : NodeBase {
    private string _clientName = "";
    private Type? _clientType;
    private string _desc = "";
    private string _url = "";

    protected RootNode(ProjectTypes.ClientGroup cg, Type? clientType) {
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
        if (clientType != null) {
            this.OpenClientConfigCommand = this.CreateCommand(this.OnOpenClientConfig);
            GlobalHub.subscribe<MessageProject.MsgClientUpdated>(this.OnClientUpdated).Then(this.MarkForCleanup);
        }
    }

    protected RootNode() {
        this.Client = ProjectTypes.ClientGroup.Empty();
        this.ClientConfigFile = "";
        this.ServiceName = "";
    }

    public ProjectTypes.ClientGroup Client { get; protected set; }

    public Type? ClientType {
        get => this._clientType;
        set => this.RaiseAndSetIfChanged(ref this._clientType, value);
    }

    public ICommand? OpenClientConfigCommand { get; }
    public string ClientConfigFile { get; protected set; }
    public string ClientPath { get; protected set; } = "";

    public string ClientName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref this._clientName, value);
    }

    public string ServiceName { get; protected set; }

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }

    public string Desc {
        get => this._desc;
        set => this.RaiseAndSetIfChanged(ref this._desc, value);
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
    }
}