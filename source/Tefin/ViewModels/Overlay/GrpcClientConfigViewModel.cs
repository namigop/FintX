#region

using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;

#endregion

namespace Tefin.ViewModels.Overlay;

public class GrpcClientConfigViewModel : ViewModelBase, IOverlayViewModel {
    private readonly string _clientConfigFile;
    private readonly Action _onClientNameChanged;
    private string _certFile = "";
    private ProjectTypes.ClientConfig _clientConfig = null!;
    private string _clientName = "";
    private string _description = "";
    private bool _isCertFromFile;
    private bool _isUsingSsl;
    private string _jwt = "";
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

        this.Load(this._clientConfigFile);
    }

    public ICommand CancelCommand { get; }

    public string CertFile {
        get => this._certFile;
        set => this.RaiseAndSetIfChanged(ref this._certFile, value);
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

    public ICommand ResetCommand { get; }
    public ICommand OpenCertFileCommand { get; }

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

    public string Title {
        get;
    } = "Client Configuration";

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private void Load(string clientConfigFile) {
        this._clientConfig = new ReadClientConfigFeature(clientConfigFile, this.Io).Read();
        this.ClientName = this._clientConfig.Name;
        this.Url = this._clientConfig.Url;
        this.IsUsingSsl = this._clientConfig.IsUsingSSL;

        this.IsCertFromFile = this._clientConfig.IsCertFromFile;
        this.Jwt = this._clientConfig.Jwt;
        this.Description = this._clientConfig.Description;
        if (string.IsNullOrWhiteSpace(this._clientConfig.CertStoreLocation)) {
            this.SelectedCertStoreLocation = StoreLocation.LocalMachine;
        }
        else {
            this.SelectedCertStoreLocation =
                Enum.Parse<StoreLocation>(this._clientConfig.CertStoreLocation);
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

        var feature = new SaveClientConfigFeature(this._clientConfigFile, this._clientConfig, this.Io);
        await feature.Save();
        if (nameChanged) {
            this._onClientNameChanged();
        }

        this.Close();
    }

    private Task OnOpenCertFile() {
        this.Io.Log.Error(new NotImplementedException()); //TODO
        return Task.CompletedTask;
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