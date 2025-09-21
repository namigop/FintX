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
using Tefin.ViewModels.Explorer.Client;

using ClientCompiler = Tefin.Core.Build.ClientCompiler;

namespace Tefin.ViewModels.Explorer.ServiceMock;

public class ServiceMockRootNode : NodeBase {
    private bool _compileInProgress;
    private bool _sessionLoaded;
    
    public ServiceMockRootNode(ProjectTypes.ServiceMockGroup cg, Type? serviceBaseType) {
        // this.Mock = ProjectTypes.ClientGroup.Empty();
        // this.CanOpen = true;
        // this.ClientType = clientType;
        // this.ClientName = "";
        // this.ClientConfigFile = "";
        // this.ClientPath = "";
        // this.ServiceName = "";

        this.ServiceType = serviceBaseType;
        this.Update(cg);

        this.IsExpanded = true;
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        //this.ImportCommand = this.CreateCommand(this.OnImport);
        //this.ExportCommand = this.CreateCommand(this.OnExport);
        GlobalHub.subscribe<MessageProject.MsgServiceMockUpdated>(this.OnServiceMockUpdated)
            .Then(this.MarkForCleanup);
    }

    
    public ICommand CompileCommand { get; }

    public ICommand DeleteCommand { get; }

    public bool IsLoaded => this.Items.Count > 0 && this.Items[0] is not EmptyNode && this._sessionLoaded;

     
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
            new LoadSessionFeature(
                this.ServiceMockGroup.Path,
                this.Items.Cast<MethodNode>(),
                this.Io,
                loaded => {
                    this._sessionLoaded = loaded;
                    this.RaisePropertyChanged(nameof(this.IsLoaded));
                });

        DispatcherTimer.RunOnce(loadSessionFeature.Run, TimeSpan.FromMilliseconds(100));
    }

    public Type ServiceType { get; private set; }

    private void OnClientNameChanged() {
    }

    private void OnServiceMockUpdated(MessageProject.MsgServiceMockUpdated obj) {
        //update in case the Url and ClientName has been changed
        if (this.ServicePath == obj.Path || this.ServicePath == obj.PreviousPath) {
            var cg = obj.Client;
            this.Update(cg);
        }
    }

    private async Task OnCompile() {
        if (this._compileInProgress) {
            return;
        }

        try {
            this._compileInProgress = true;
            var protoFiles = Array.Empty<string>();
            var compile = new CompileFeature(this.ServiceName, this.ServiceName, this.Desc, protoFiles, this.Url,
                this.Io);
            var csFiles = this.ServiceMockGroup.CodeFiles;
            var (ok, compileOutput) = await compile.CompileExisting(csFiles);
            if (ok) {
                var types = ClientCompiler.getTypes(compileOutput.CompiledBytes);
                var clientTypes = ServiceClient.findClientType(types);
                if (clientTypes.Length == 0) {
                    throw new Exception("No client types found");
                }
                if (clientTypes.Length == 1) {
                    this.ServiceType = clientTypes[0];
                }
                else {
                    this.ServiceType = clientTypes.First(c => {
                        var svc = c.DeclaringType!.FullName!.ToUpperInvariant();
                        return svc.EndsWith(this.ServiceName.ToUpperInvariant());
                    });
                }

                this.Init();
            }
        }
        finally {
            this._compileInProgress = false;
        }
    }

    private void OnDelete() {
        var feature = new DeleteServiceMockFeature(this.ServiceMockGroup, this.Io);
        feature.Delete();

        foreach (var m in this.Items) {
            GlobalHub.publish(new RemoveTreeItemMessage(m));
        }

        this.Items.Clear();
        GlobalHub.publish(new ServiceMockDeletedMessage(this.ServiceMockGroup));
    }

    private void Update(ProjectTypes.ServiceMockGroup cg) {
        this.ServiceMockGroup = cg;
        this.CanOpen = true;
        this.ServicePath = cg.Path;
        this.ServiceName = cg.Config.Value.ServiceName;
        // this.Url = cg.Config.Value.Url;
         this.Title = cg.Config.Value.ServiceName;
        // this.SubTitle = cg.Config.Value.Description;
        // this.Desc = cg.Config.Value.Description;
        this.ServiceConfigFile = cg.ConfigFile;
 
    }

    public ProjectTypes.ServiceMockGroup ServiceMockGroup { get; private set; }

    public string Desc { get; set; }

    public string ServiceConfigFile { get; private set; }

    public string Url { get; private set; }

    public string ServicePath { get; private set; }
    public string ServiceName { get; private set; }
    public uint Port { get; private set; }
    public ICommand OpenServiceMockConfigCommand { get; }
}