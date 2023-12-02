using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;

namespace Tefin.ViewModels.Overlay;

public class GrpcClientConfigViewModel : ViewModelBase, IOverlayViewModel {
    private readonly string _clientConfigFile;
    private readonly Action _onClientNameChanged;
    private readonly string _title = "Client Configuration";
    private string _certFile;
    private ProjectTypes.ClientConfig _clientConfig;
    private string _clientName;
    private string _description;
    private bool _isCertFromFile;
    private bool _isUsingSsl;
    private string _jwt;
    private StoreLocation _selectedCertStoreLocation;
    private StoreCertSelection _selectedStoreCertificate;
    private string _thumbprint;
    private string _url;

    public GrpcClientConfigViewModel(string clientConfigFile, Action onClientNameChanged) {
        this._clientConfigFile = clientConfigFile;
        this._onClientNameChanged = onClientNameChanged;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.OpenCertFileCommand = this.CreateCommand(this.OnOpenCertFile);
        this.LoadStoreCertificatesCommand = this.CreateCommand(this.OnLoadStoreCertificates);
        this.CertStoreLocations.Add(StoreLocation.CurrentUser);
        this.CertStoreLocations.Add(StoreLocation.LocalMachine);
        this._selectedCertStoreLocation = this.CertStoreLocations[1];

        Load(this._clientConfigFile);
    }

    public ICommand CancelCommand { get; }

    public string CertFile {
        get => this._certFile;
        set => this.RaiseAndSetIfChanged(ref _certFile, value);
    }

    public List<StoreLocation> CertStoreLocations { get; } = new();

    public string ClientName {
        get => this._clientName;
        set => this.RaiseAndSetIfChanged(ref _clientName, value);
    }

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    public bool IsCertFromFile {
        get => this._isCertFromFile;
        set => this.RaiseAndSetIfChanged(ref _isCertFromFile, value);
    }

    public bool IsUsingSSL {
        get => this._isUsingSsl;
        set => this.RaiseAndSetIfChanged(ref _isUsingSsl, value);
    }

    public string JWT {
        get => this._jwt;
        set => this.RaiseAndSetIfChanged(ref _jwt, value);
    }

    public ICommand LoadStoreCertificatesCommand { get; }

    public ICommand OkayCommand { get; }

    public ICommand OpenCertFileCommand { get; }

    public StoreLocation SelectedCertStoreLocation {
        get => this._selectedCertStoreLocation;
        set {
            this.RaiseAndSetIfChanged(ref _selectedCertStoreLocation, value);
            this.Thumbprint = "";
        }
    }

    public StoreCertSelection SelectedStoreCertificate {
        get => this._selectedStoreCertificate;
        set {
            this.RaiseAndSetIfChanged(ref _selectedStoreCertificate, value);
            this.Thumbprint = this._selectedStoreCertificate?.Thumbprint ?? string.Empty;
        }
    }

    public ObservableCollection<StoreCertSelection> StoreCertificates { get; } = new();

    public string Thumbprint {
        get => this._thumbprint;
        set => this.RaiseAndSetIfChanged(ref _thumbprint, value);
    }

    public string Title => this._title;

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref _url, value);
    }

    public void Close() {
        GlobalHub.publish(new CloseOverlayMessage(this));
    }

    private void Load(string clientConfigFile) {
        this._clientConfig = new ReadClientConfigFeature(clientConfigFile, this.Io).Read();
        this.ClientName = this._clientConfig.Name;
        this.Url = this._clientConfig.Url;
        this.IsUsingSSL = this._clientConfig.IsUsingSSL;

        this.IsCertFromFile = this._clientConfig.IsCertFromFile;
        this.JWT = this._clientConfig.Jwt;
        this.Description = this._clientConfig.Description;
        if (string.IsNullOrWhiteSpace(this._clientConfig.CertStoreLocation)) {
            this.SelectedCertStoreLocation = StoreLocation.LocalMachine;
        }
        else {
            this.SelectedCertStoreLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), this._clientConfig.CertStoreLocation);
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
        this._clientConfig.IsCertFromFile = this.IsUsingSSL && this.IsCertFromFile;
        this._clientConfig.Jwt = this.JWT;
        this._clientConfig.IsUsingSSL = this.IsUsingSSL;
        this._clientConfig.Description = this.Description;
        this._clientConfig.CertStoreLocation = Enum.GetName(this.SelectedCertStoreLocation);
        this._clientConfig.CertThumbprint = this.Thumbprint;

        var feature = new SaveClientConfigFeature(this._clientConfigFile, this._clientConfig, Io);
        await feature.Save();
        if (nameChanged) {
            this._onClientNameChanged();
        }

        this.Close();
    }

    private Task OnOpenCertFile() {
        Io.Log.Error(new NotImplementedException()); //TODO
        return Task.CompletedTask;
    }

    public class StoreCertSelection(string subject, string thumbprint) {
        public string Subject { get => subject; }
        public string Thumbprint { get => thumbprint; }
    }
}