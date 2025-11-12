using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Features.Scripting;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Overlay;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;

namespace Tefin.ViewModels.Explorer.ServiceMock;

public class ServiceMockRootNode : NodeBase {
    private bool _canStartServer;
    private bool _compileInProgress;
    private ServerHost? _host;
    private uint _port;
    private bool _sessionLoaded;

    public ServiceMockRootNode(ProjectTypes.ServiceMockGroup cg, Type? serviceBaseType) {
        this.ServiceType = serviceBaseType;
        this.Update(cg);
        this.IsExpanded = true;
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this.RecompileCommand = this.CreateCommand(this.OnRecompile);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        this.StartServerCommand = this.CreateCommand(this.OnStartServer);
        this.StopServerCommand = this.CreateCommand(this.OnStopServer);
        this.OpenServiceMockConfigCommand = this.CreateCommand(this.OnOpenServiceMockConfig);
        //this.ImportCommand = this.CreateCommand(this.OnImport);
        //this.ExportCommand = this.CreateCommand(this.OnExport);
        GlobalHub.subscribe<MessageProject.MsgServiceMockUpdated>(this.OnServiceMockUpdated)
            .Then(this.MarkForCleanup);
    }

    public bool CanStartServer {
        get => this._canStartServer;
        private set => this.RaiseAndSetIfChanged(ref this._canStartServer, value);
    }

    public ICommand CompileCommand { get; }

    public ICommand DeleteCommand { get; }

    public string Desc { get; set; }

    public bool IsLoaded => this.Items.Count > 0 && this.Items[0] is not EmptyNode;

    public bool IsRunning => this._host is { IsRunning: true };

    public bool IsUsingNamedPipes { get; private set; }

    public ICommand OpenServiceMockConfigCommand { get; }

    public string PipeName { get; private set; }

    public uint Port {
        get => this._port;
        private set => this.RaiseAndSetIfChanged(ref this._port, value);
    }

    public ICommand RecompileCommand { get; }

    public string ServiceConfigFile { get; private set; }

    public ProjectTypes.ServiceMockGroup ServiceMockGroup { get; private set; }
    public string ServiceName { get; private set; }

    public string ServicePath { get; private set; }

    public Type? ServiceType { get; private set; }

    public ICommand StartServerCommand { get; }

    public ICommand StopServerCommand { get; }

    public string Url { get; private set; }

    private async Task Compile(bool recompile) {
        if (this._compileInProgress) {
            return;
        }

        try {
            this._compileInProgress = true;
            var protoFiles = Array.Empty<string>();
            var compile = new CompileFeature(this.ServiceName, $"{this.ServiceName}DummyClient", this.Desc, protoFiles,
                this.Url,
                this.Io);
            var csFiles = this.ServiceMockGroup.CodeFiles;
            var (ok, compileOutput) = await compile.CompileExisting(csFiles, true, recompile);
            if (ok) {
                var types = ClientCompiler.getTypes(compileOutput.CompiledBytes);
                var serviceImplTypes = ServiceClient.findConcreteServiceTypes(types);
                if (serviceImplTypes.Length == 0) {
                    throw new Exception("No client types found");
                }

                if (serviceImplTypes.Length == 1) {
                    this.ServiceType = serviceImplTypes[0];
                }
                else {
                    this.ServiceType = serviceImplTypes.First(c => {
                        var svc = c.DeclaringType!.FullName!.ToUpperInvariant();
                        return svc.EndsWith(this.ServiceName.ToUpperInvariant());
                    });
                }

                this.Init();
            }
        }
        finally {
            this.CanStartServer = this.ServiceType != null;
            this._compileInProgress = false;
        }
    }


    public override void Init() {
        if (this.ServiceType == null) {
            return;
        }

        //if (this.Items.FirstOrDefault() is EmptyNode)
        foreach (var i in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(i));
        }

        this.Items.Clear();

        var methodInfos = ServiceClient.findMethods(this.ServiceType);
        foreach (var m in methodInfos) {
            var item = new MockMethodNode(m, this.ServiceMockGroup);
            item.Init();
            this.AddItem(item);
            Log.logInfo.Invoke($"Found method {item.Title}");
        }

        this.IsExpanded = true;
        var loadSessionFeature =
            new LoadScriptSessionFeature(
                this.ServiceMockGroup.Path,
                this.Items.Cast<MockMethodNode>(),
                this.Io,
                loaded => {
                    this._sessionLoaded = loaded;
                    this.RaisePropertyChanged(nameof(this.IsLoaded));
                });
        loadSessionFeature.Run();
    }

    private async Task OnCompile() => await this.Compile(false);

    private void OnDelete() {
        var feature = new DeleteServiceMockFeature(this.ServiceMockGroup, this.Io);
        feature.Delete();

        foreach (var m in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(m));
        }

        this.Items.Clear();
        GlobalHub.publish(new ServiceMockDeletedMessage(this.ServiceMockGroup));
    }

    private void OnOpenServiceMockConfig() {
        var vm = new GrpcServiceMockConfigViewModel(this.ServiceConfigFile, this.OnServiceMockNameChanged);
        GlobalHub.publish(new OpenOverlayMessage(vm));
    }

    private async Task OnRecompile() => await this.Compile(true);

    private void OnServiceMockNameChanged() {
    }

    private void OnServiceMockUpdated(MessageProject.MsgServiceMockUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.ServicePath == obj.Path || this.ServicePath == obj.PreviousPath) {
            var cg = obj.Client;
            this.Update(cg);
        }
    }

    private async Task OnStartServer() {
        try {
            this.CanStartServer = false;
            this._host = new ServerHost(this.ServiceType, this.Port, this.ServiceName, this.IsUsingNamedPipes, this.PipeName);
            await this._host.Start();
            this.Io.Log.Info($"{this.ServiceName} server started.");
        }
        catch (Exception ex) {
            this.Io.Log.Error(ex);
            this.CanStartServer = true;
        }
        finally {
            this.RaisePropertyChanged(nameof(this.IsRunning));
        }
    }

    private async Task OnStopServer() {
        try {
            await this._host?.Stop()!;
            this.CanStartServer = true;
            this.Io.Log.Info($"{this.ServiceName} server stopped.");
        }
        catch (Exception ex) {
            this.Io.Log.Error(ex);
        }
        finally {
            this.RaisePropertyChanged(nameof(this.IsRunning));
        }
    }

    private void Update(ProjectTypes.ServiceMockGroup cg) {
        this.ServiceMockGroup = cg;
        this.CanOpen = true;
        this.ServicePath = cg.Path;
        this.ServiceName = cg.Config.Value.ServiceName;
        this.Port = cg.Config.Value.Port;
        this.IsUsingNamedPipes = cg.Config.Value.IsUsingNamedPipes;
        this.PipeName = cg.Config.Value.PipeName;
        // this.Url = cg.Config.Value.Url;
        this.Title = cg.Config.Value.ServiceName;
        // this.SubTitle = cg.Config.Value.Description;
        // this.Desc = cg.Config.Value.Description;
        this.ServiceConfigFile = cg.ConfigFile;
    }
}