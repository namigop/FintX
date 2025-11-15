#region

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Validations;

#endregion

//using Tefin.Core.ServiceClient;

namespace Tefin.ViewModels.Overlay;

public class AddGrpcMockOverlayViewModel : ViewModelBase, IOverlayViewModel {
    private readonly ProjectTypes.Project _project;
    private string _address = string.Empty;
    private string _clientName = string.Empty;
    private bool _isDiscoveringUsingProto;
    private bool _isUsingNamedPipes;
    private string _pipeName;
    private bool _isUsingUnixDomainSockets;
    private string _socketFileName;
    private string _port = "50051";
    private string _protoFile = string.Empty;
    private string _protoFilesOrUrl = string.Empty;
    private string? _selectedDiscoveredService;

    public AddGrpcMockOverlayViewModel(ProjectTypes.Project project) {
        this._project = project;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.DiscoverCommand = this.CreateCommand(this.OnDiscover);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.ReflectionUrl = StartDemoGrpcServiceFeature.Url;
        this.Title = "Create a mock gRPC service";
        this.Description = string.Empty;
    }

    public ICommand CancelCommand { get; }

    public string Description { get; set; }
    public ICommand DiscoverCommand { get; }
    public ObservableCollection<string> DiscoveredServices { get; } = [];

    public bool IsDiscoveringUsingProto {
        get => this._isDiscoveringUsingProto;
        set => this.RaiseAndSetIfChanged(ref this._isDiscoveringUsingProto, value);
    }

    public bool IsUsingNamedPipes {
        get => this._isUsingNamedPipes;
        set => this.RaiseAndSetIfChanged(ref this._isUsingNamedPipes, value);
    }
    public bool IsUsingUnixDomainSockets {
        get => this._isUsingUnixDomainSockets;
        set => this.RaiseAndSetIfChanged(ref this._isUsingUnixDomainSockets, value);
    }

    public ICommand OkayCommand { get; }

    public string PipeName {
        get => this._pipeName;
        set => this.RaiseAndSetIfChanged(ref this._pipeName, value);
    }
    public string SocketFileName {
        get => this._socketFileName;
        set => this.RaiseAndSetIfChanged(ref this._socketFileName, value);
    }

    [Required(ErrorMessage = "Enter a port number")]
    [IsValidPortNumber]
    public string Port {
        get => this._port;
        set => this.RaiseAndSetIfChanged(ref this._port, value);
    }

    [IsProtoFile]
    public string ProtoFile {
        get => this._protoFile;
        set => this.RaiseAndSetIfChanged(ref this._protoFile, value);
    }

    [IsHttp]
    public string ReflectionUrl {
        get => this._protoFilesOrUrl;
        set => this.RaiseAndSetIfChanged(ref this._protoFilesOrUrl, value);
    }

    [Required(ErrorMessage = "Service is required")]
    public string? SelectedDiscoveredService {
        get => this._selectedDiscoveredService;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedDiscoveredService, value);
            if (!string.IsNullOrWhiteSpace(this._selectedDiscoveredService)) {
                if (string.IsNullOrWhiteSpace(this.ServiceName)) {
                    var name = this._selectedDiscoveredService.Split(".").Last();
                    this.ServiceName = $"{name}Mock";
                }
            }
        }
    }

    [Required(ErrorMessage = "Enter a unique name")]
    [IsValidFolderName]
    public string ServiceName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref this._clientName, value);
    }

    public string Title { get; }

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private async Task OnDiscover() {
        DiscoverParameters? discoParams = null;

        if (!this.IsDiscoveringUsingProto) {
            //Discover using the reflection service
            discoParams = new DiscoverParameters([], new Uri(this.ReflectionUrl));
        }
        else {
            var (ok, files) = await DialogUtils.OpenFile("Open File", "Proto Files", ["*.proto"]);
            if (ok) {
                this.ProtoFile = files[0];
                // PopulateServiceNamesFromProto();
                discoParams = new DiscoverParameters([this.ProtoFile], null);
            }
        }

        if (discoParams == null) {
            return;
        }

        var res = await ServiceClient.discover(this.Io, discoParams);

        if (res.IsOk) {
            var services = res.ResultValue;
            if (services.Length != 0) {
                this.DiscoveredServices.Clear();
                foreach (var s in services) {
                    this.DiscoveredServices.Add(s);
                }

                this.SelectedDiscoveredService = this.DiscoveredServices.FirstOrDefault();
            }
        }
        else {
            this.Io.Log.Error(res.ErrorValue);
        }
    }

    private async Task OnOkay() {
        if (string.IsNullOrWhiteSpace(this.ServiceName)) {
            this.Io.Log.Error("Service name is empty.  Enter a valid name");
            return;
        }

        if (string.IsNullOrWhiteSpace(this.SelectedDiscoveredService)) {
            this.Io.Log.Error("Please select a service");
            return;
        }

        if (this._project.Mocks.FirstOrDefault(t => t.Name == this.ServiceName) != null) {
            this.Io.Log.Error("Mock service already exists.  Enter a new name");
            return;
        }

        this.Close();

        var protoFiles = this.IsDiscoveringUsingProto ? new[] { this.ProtoFile } : Array.Empty<string>();
        var disco = new DiscoverFeature(protoFiles, this.ReflectionUrl);
        var (success, _) = await disco.Discover(this.Io);
        if (success) {
            var cmd = new CompileFeature(this._selectedDiscoveredService!, this._clientName, "desc", protoFiles,
                this.ReflectionUrl, this.Io);
            var (ok, output) = await cmd.Run(true);
            if (ok) {
                var csFiles = output.Input.Value.SourceFiles;
                var msg = new ShowServiceMockMessage(
                    output,
                    this.IsDiscoveringUsingProto ? protoFiles[0] : this.ReflectionUrl,
                    this.ServiceName,
                    this.SelectedDiscoveredService,
                    this.Description,
                    csFiles,
                    Convert.ToUInt32(this.Port),
                    this.IsUsingNamedPipes,
                    this.PipeName,
                    this.IsUsingUnixDomainSockets,
                    this.SocketFileName,
                    output.Input.Value.ModuleFile) { Reset = false };
                GlobalHub.publish(msg);
            }
        }
    }
}