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

namespace Tefin.ViewModels.Overlay;

public class ResetGrpcServiceOverlayViewModel : ViewModelBase, IOverlayViewModel {
    private readonly ProjectTypes.ClientConfig _clientConfig;
    private readonly string _clientPath;
    private string _address = string.Empty;
    private string _clientName = string.Empty;
    private bool _isDiscoveringUsingProto;
    private string _protoFile = string.Empty;
    private string _protoFilesOrUrl = string.Empty;
    private string? _selectedDiscoveredService;

    public ResetGrpcServiceOverlayViewModel(string clientConfigFile) {
        this._clientConfig = new ReadClientConfigFeature(clientConfigFile, this.Io).Read();
        this._clientPath = Path.GetDirectoryName(clientConfigFile)!;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.DiscoverCommand = this.CreateCommand(this.OnDiscover);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.ReflectionUrl = this._clientConfig.Url;
        this.Title = "Reset gRPC client";
        this.ClientName = this._clientConfig.Name;
        this.Description = this._clientConfig.Description;
    }

    [IsHttp]
    public string Address {
        get => this._address;
        set => this.RaiseAndSetIfChanged(ref this._address, value);
    }

    public ICommand CancelCommand { get; }

    [Required(ErrorMessage = "Enter a unique name")]
    [IsValidFolderName]
    public string ClientName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref this._clientName, value);
    }

    public string Description { get; set; }
    public ICommand DiscoverCommand { get; }
    public ObservableCollection<string> DiscoveredServices { get; } = [];

    public bool IsDiscoveringUsingProto {
        get => this._isDiscoveringUsingProto;
        set => this.RaiseAndSetIfChanged(ref this._isDiscoveringUsingProto, value);
    }

    public ICommand OkayCommand { get; }

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
            if (string.IsNullOrWhiteSpace(this._selectedDiscoveredService)) {
                return;
            }

            if (string.IsNullOrWhiteSpace(this.ClientName)) {
                var name = this._selectedDiscoveredService.Split(".").Last();
                this.ClientName = $"{name}Client";
            }

            if (string.IsNullOrWhiteSpace(this.Address)) {
                this.Address = "http://";
            }
        }
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
            if (services.Any()) {
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
        if (string.IsNullOrWhiteSpace(this.ClientName)) {
            this.Io.Log.Error("Client name is empty.  Enter a valid name");
            return;
        }

        if (string.IsNullOrWhiteSpace(this.SelectedDiscoveredService)) {
            this.Io.Log.Error("Please select a service");
            return;
        }

        this.Close();

        var protoFiles = this.IsDiscoveringUsingProto
            ? new[] { this.ProtoFile }
            : Array.Empty<string>();
        var disco = new DiscoverFeature(protoFiles, this.ReflectionUrl);
        var (serviceFound, _) = await disco.Discover(this.Io);
        if (serviceFound) {
            var cfgGroup = new LoadClientFeature(this.Io, this._clientPath).Run();

            // remove old *.cs and *.dll files
            foreach (var f in cfgGroup.CodeFiles) {
                this.Io.File.Delete(f);
            }

            var cmd = new CompileFeature(this._selectedDiscoveredService!, this._clientName, "desc", protoFiles,
                this.ReflectionUrl, this.Io);
            var (ok, output) = await cmd.Run(false);
            if (ok) {
                var csFiles = output.Input.Value.SourceFiles;
                var address = this.IsDiscoveringUsingProto ? this.Address : this.ReflectionUrl;

                address = string.IsNullOrWhiteSpace(address) ? "http://address/not/set" : address;
                var msg = new ShowClientMessage(output,
                    address,
                    this.ClientName,
                    this.SelectedDiscoveredService,
                    this.Description,
                    csFiles,
                    output.Input.Value.ModuleFile) { Reset = true };

                GlobalHub.publish(msg);
            }
        }
    }
}