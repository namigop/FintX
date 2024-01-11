#region

using System.Windows.Input;

using Avalonia.Threading;

using Newtonsoft.Json.Linq;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Overlay;
using Tefin.ViewModels.Tabs;

using static Tefin.Core.Interop.Messages;

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
        GlobalHub.subscribe<MsgClientUpdated>(this.OnClientUpdated);
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

    public ProjectTypes.ClientGroup Client {
        get;
        private set;
    }

    public bool IsLoaded {
        get => this.Items.Count > 0 && this.Items[0] is not EmptyNode && this._sessionLoaded;
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
            var item = new MethodNode(m, this.Client);
            item.Init();
            this.AddItem(item);
            Log.logInfo.Invoke($"Found method {item.Title}");
        }

        this.IsExpanded = true;

        DispatcherTimer.RunOnce(this.TryLoadPreviousSession, TimeSpan.FromMilliseconds(100));
        this.RaisePropertyChanged(nameof(this.IsLoaded));

    }
    private void TryLoadPreviousSession() {
        void LoadOne(string json, string reqFile) {
            var methodName = Core.Utils.jSelectToken(json, "$.Method").Value<string>();
            var item = this.Items.Cast<MethodNode>().FirstOrDefault(i => i.MethodInfo.Name == methodName);
            var tab = TabFactory.From(item, this.Io, reqFile);
            if (tab != null)
                GlobalHub.publish(new OpenTabMessage(tab));
        }


        AutoSave.getAutoSavedFiles(this.Io, this.Client.Path)
            .Select(reqFile => {
                var json = this.Io.File.ReadAllText(reqFile);
                if (string.IsNullOrWhiteSpace(json))
                    return null;
                return new Action(() => LoadOne(json, reqFile));
            })
            .Where(a => a != null)
            .ToArray()
            .Then(actions => {
                if (actions.Any()) {
                    var pos = 0;
                    DispatcherTimer.Run(
                        () => {
                            if (pos < actions.Length) {
                                actions[pos].Invoke();
                                pos += 1;
                                return true;
                            }

                            this._sessionLoaded = true;
                            return false;
                        },
                        TimeSpan.FromMilliseconds(100));
                }
                else {
                    this._sessionLoaded = true;
                }
            });
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
        if (this._compileInProgress)
            return;

        try {
            this._compileInProgress = true;
            var protoFiles = Array.Empty<string>(); //TODO
            var compile = new CompileFeature(this.ServiceName, this.ClientName, this.Desc, protoFiles, this.Url, this.Io);
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