#region

using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;
using Tefin.Utils;
using Tefin.ViewModels.Validations;

#endregion

namespace Tefin.ViewModels.Overlay;

public class GrpcClientConfigViewModel : ViewModelBase, IOverlayViewModel {
    private readonly string _clientConfigFile;
    private readonly Action _onClientNameChanged;
    private string _certFile = "";
    private string _certFilePassword = "";
    private ProjectTypes.ClientConfig _clientConfig = null!;
    private string _clientName = "";
    private string _description = "";
    private bool _isCertFromFile;
    private bool _isUsingSsl;
    private string _jwt = "";
    private bool _requiresPassword;
    private StoreLocation _selectedCertStoreLocation;
    private StoreCertSelection _selectedStoreCertificate = null!;
    private string _thumbprint = "";
    private string _url = "";
    
    public GrpcClientConfigViewModel(string clientConfigFile, Action onClientNameChanged) {
        this._clientConfigFile = clientConfigFile;
        this._onClientNameChanged = onClientNameChanged;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.OpenCertFileCommand = this.CreateCommand(this.OnOpenCertFile);
        this.ResetCommand = this.CreateCommand(this.OnReset);
        this.LoadStoreCertificatesCommand = this.CreateCommand(this.OnLoadStoreCertificates);
        this.CertStoreLocations.Add(StoreLocation.CurrentUser);
        this.CertStoreLocations.Add(StoreLocation.LocalMachine);
        this._selectedCertStoreLocation = this.CertStoreLocations[1];
        this.TransportOptions = new TransportOptionsViewModel();
        
        this.Load(this._clientConfigFile);
        this.TransportOptions.SubscribeTo<string, TransportOptionsViewModel>(
            x => x.SelectedTransport,
            vm => {
                 if (vm.IsUsingNamedPipes || vm.IsUsingUnixDomainSockets) {
                     this.Url = "http://localhost";
                 }
            });
    }

    public TransportOptionsViewModel TransportOptions { get; }

    public ICommand CancelCommand { get; }

    [FileExists]
    public string CertFile {
        get => this._certFile;
        set {
            this.RaiseAndSetIfChanged(ref this._certFile, value);
            this.RequiresPassword = Path.GetExtension(value).ToLower() == Ext.pfxExt;
        }
    }

    public string CertFilePassword {
        get => this._certFilePassword;
        set => this.RaiseAndSetIfChanged(ref this._certFilePassword, value);
    }

    public List<StoreLocation> CertStoreLocations { get; } = new();

    public string ClientName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref this._clientName, value);
    }

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref this._description, value);
    }

    public bool IsCertFromFile {
        get => this._isCertFromFile;
        set => this.RaiseAndSetIfChanged(ref this._isCertFromFile, value);
    }

    public bool IsUsingSsl {
        get => this._isUsingSsl;
        set => this.RaiseAndSetIfChanged(ref this._isUsingSsl, value);
    }

    public string Jwt {
        get => this._jwt;
        set => this.RaiseAndSetIfChanged(ref this._jwt, value);
    }

    public ICommand LoadStoreCertificatesCommand { get; }

    public ICommand OkayCommand { get; }
    public ICommand OpenCertFileCommand { get; }

    public bool RequiresPassword {
        get => this._requiresPassword;
        private set => this.RaiseAndSetIfChanged(ref this._requiresPassword, value);
    }

    public ICommand ResetCommand { get; }

    public StoreLocation SelectedCertStoreLocation {
        get => this._selectedCertStoreLocation;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedCertStoreLocation, value);
            this.Thumbprint = "";
        }
    }

    public StoreCertSelection SelectedStoreCertificate {
        get => this._selectedStoreCertificate;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedStoreCertificate, value);
            this.Thumbprint = this._selectedStoreCertificate?.Thumbprint ?? string.Empty;
        }
    }

    public ObservableCollection<StoreCertSelection> StoreCertificates { get; } = new();

    public string Thumbprint {
        get => this._thumbprint;
        set => this.RaiseAndSetIfChanged(ref this._thumbprint, value);
    }

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }

    public string Title { get; } = "Client Configuration";

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private void Load(string clientConfigFile) {
        this._clientConfig = new ReadClientConfigFeature(clientConfigFile, this.Io).Read();
        this.ClientName = this._clientConfig.Name;
        this.Url = this._clientConfig.Url;
        this.Jwt = this._clientConfig.Jwt;
        this.IsUsingSsl = this._clientConfig.IsUsingSSL;
        this.IsCertFromFile = this._clientConfig.IsCertFromFile;
        this.Description = this._clientConfig.Description;
        if (this._clientConfig.IsUsingNamedPipes) {
            this.TransportOptions.SelectedTransport = TransportOptionsViewModel.NamedPipes;
            this.TransportOptions.SocketOrPipeName = this._clientConfig.NamedPipe.PipeName;
        }
        else if (this._clientConfig.IsUsingUnixDomainSockets) {
            this.TransportOptions.SelectedTransport = TransportOptionsViewModel.UnixDomainSockets;
            this.TransportOptions.SocketOrPipeName = this._clientConfig.UnixDomainSockets.SocketFileName;
        }
        else {
            this.TransportOptions.SelectedTransport = TransportOptionsViewModel.Default;
        }
         
        if (this.IsCertFromFile) {
            this.CertFile = this._clientConfig.CertFile;
            this.CertFilePassword = Core.Utils.decrypt(
                this._clientConfig.CertFilePassword,
                Path.GetFileName(this._clientConfig.CertFile));
            ;
        }
        else {
            this.SelectedCertStoreLocation = string.IsNullOrWhiteSpace(this._clientConfig.CertStoreLocation)
                ? StoreLocation.LocalMachine
                : Enum.Parse<StoreLocation>(this._clientConfig.CertStoreLocation);
        }

        this.Thumbprint = this._clientConfig.CertThumbprint;
    }

    private void OnLoadStoreCertificates() {
        var store = new X509Store(this.SelectedCertStoreLocation);
        store.Open(OpenFlags.ReadOnly);
        var certificates = store.Certificates;
        this.StoreCertificates.Clear();
        foreach (var certificate in certificates) {
            var subject = certificate.SubjectName.Name;
            var thumbprint = certificate.Thumbprint;
            var selection = new StoreCertSelection(subject, thumbprint);
            this.StoreCertificates.Add(selection);
            //this.StoreCertificates.Add($"FriendlyName: {friendlyName}, Thumb:{thumbprint}");
        }
    }

    private async Task OnOkay() {
        //save as json to proj
        var nameChanged = this._clientConfig.Name != this.ClientName;
        this._clientConfig.Name = this.ClientName;
        this._clientConfig.Url = this.Url;
        this._clientConfig.IsCertFromFile = this.IsUsingSsl && this.IsCertFromFile;
        this._clientConfig.Jwt = this.Jwt;
        this._clientConfig.IsUsingSSL = this.IsUsingSsl;
        this._clientConfig.Description = this.Description;
        this._clientConfig.CertStoreLocation = Enum.GetName(this.SelectedCertStoreLocation);
        this._clientConfig.CertThumbprint = this.Thumbprint;
        this._clientConfig.CertFile = this.CertFile;
        if (this.TransportOptions.IsUsingNamedPipes) {
            this._clientConfig.IsUsingNamedPipes = true;
            this._clientConfig.NamedPipe.PipeName = this.TransportOptions.SocketOrPipeName;;
            this._clientConfig.IsUsingUnixDomainSockets = false;
            this._clientConfig.UnixDomainSockets.SocketFileName = "";
        }
        else if (this.TransportOptions.IsUsingUnixDomainSockets) {
            this._clientConfig.IsUsingNamedPipes = false;
            this._clientConfig.NamedPipe.PipeName = "";
            this._clientConfig.IsUsingUnixDomainSockets = true;
            this._clientConfig.UnixDomainSockets.SocketFileName = this.TransportOptions.SocketOrPipeName;
        }
        else {
            this._clientConfig.IsUsingNamedPipes = false;
            this._clientConfig.IsUsingUnixDomainSockets = false;
        }
        
        if (this._isCertFromFile) {
            if (this.RequiresPassword) {
                this._clientConfig.CertFilePassword =
                    Core.Utils.encrypt(this.CertFilePassword, Path.GetFileName(this.CertFile));
            }

            var x509 = CertUtils.createFromFile(this.CertFile, this.CertFilePassword);
            this.Thumbprint = x509.Thumbprint;
            this._clientConfig.CertThumbprint = x509.Thumbprint;
        }

        var feature = new SaveClientConfigFeature(this._clientConfigFile, this._clientConfig, this.Io);
        await feature.Save();
        if (nameChanged) {
            this._onClientNameChanged();
        }

        this.Close();
    }

    private async Task OnOpenCertFile() {
        var fileExtensions = new[] { $"*{Ext.cerExt}", $"*{Ext.pfxExt}" };
        var fileTitle = "Certificate files (*.cer, *.pfx)";
        var (ok, files) = await DialogUtils.OpenFile("Open certificate file", fileTitle, fileExtensions);
        if (ok) {
            try {
                var certFile = files[0];
                this.CertFile = certFile;
                this.IsCertFromFile = true;
                this.IsUsingSsl = true;
                this.SelectedCertStoreLocation = StoreLocation.CurrentUser;
                this.SelectedStoreCertificate = null;
            }
            catch (Exception ex) {
                this.Io.Log.Error($"Failed to load certificate: {ex.Message}");
            }
        }
    }

    private Task OnReset() {
        GlobalHub.publish(new CloseOverlayMessage(this));
        var vm = new ResetGrpcServiceOverlayViewModel(this._clientConfigFile);
        GlobalHub.publish(new OpenOverlayMessage(vm));

        return Task.CompletedTask;
    }

    public class StoreCertSelection(string subject, string thumbprint) {
        public string Subject => subject;
        public string Thumbprint => thumbprint;
    }
}