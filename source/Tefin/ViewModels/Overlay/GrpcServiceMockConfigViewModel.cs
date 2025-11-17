#region

using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.Messages;

#endregion

namespace Tefin.ViewModels.Overlay;

public class GrpcServiceMockConfigViewModel : ViewModelBase, IOverlayViewModel {
    private readonly string _mockConfigFile;
    private readonly Action _onServiceMockNameChanged;
    private string _description = "";
    private ProjectTypes.ServiceMockConfig _mockConfig = null!;
    private uint _port;
    private string _serviceMockName = "";
    private string _url = "";

    public GrpcServiceMockConfigViewModel(string mockConfigFile, Action onServiceMockNameChanged) {
        this._mockConfigFile = mockConfigFile;
        this._onServiceMockNameChanged = onServiceMockNameChanged;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.ResetCommand = this.CreateCommand(this.OnReset);
        this.TransportOptions = new TransportOptionsViewModel();
        this.Load(this._mockConfigFile);
    }

    public ICommand CancelCommand { get; }

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref this._description, value);
    }

    public ICommand OkayCommand { get; }

    public uint Port {
        get => this._port;
        set => this.RaiseAndSetIfChanged(ref this._port, value);
    }

    public ICommand ResetCommand { get; }


    public string ServiceMockName {
        get => this._serviceMockName;
        set => this.RaiseAndSetIfChanged(ref this._serviceMockName, value);
    }

    public TransportOptionsViewModel TransportOptions { get; }

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }


    public string Title { get; } = "Edit mock service configuration";

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private void Load(string mockConfigFile) {
        this._mockConfig = new ReadServiceMockConfigFeature(mockConfigFile, this.Io).Read();
        this.ServiceMockName = this._mockConfig.ServiceName;
        this.Description = this._mockConfig.Desc;
        this.Port = this._mockConfig.Port;
        if (this._mockConfig.IsUsingNamedPipes) {
            this.TransportOptions.SelectedTransport = TransportOptionsViewModel.NamedPipes;
            this.TransportOptions.SocketOrPipeName = this._mockConfig.NamedPipe.PipeName;
        }

        if (this._mockConfig.IsUsingUnixDomainSockets) {
            this.TransportOptions.SelectedTransport = TransportOptionsViewModel.UnixDomainSockets;
            this.TransportOptions.SocketOrPipeName = this._mockConfig.UnixDomainSockets.SocketFileName;
        }
    }

    private async Task OnOkay() {
        var nameChanged = this._mockConfig.ServiceName != this.ServiceMockName;
        this._mockConfig.ServiceName = this.ServiceMockName;
        this._mockConfig.Desc = this.Description;
        this._mockConfig.Port = this.Port;
        if (this.TransportOptions.IsUsingNamedPipes) {
            this._mockConfig.IsUsingNamedPipes = this.TransportOptions.IsUsingNamedPipes;
            this._mockConfig.NamedPipe.PipeName = this.TransportOptions.SocketOrPipeName;
            this._mockConfig.IsUsingUnixDomainSockets = false;
            this._mockConfig.UnixDomainSockets.SocketFileName = "";
        }

        if (this.TransportOptions.IsUsingUnixDomainSockets) {
            this._mockConfig.IsUsingNamedPipes = false;
            this._mockConfig.NamedPipe.PipeName = "";
            this._mockConfig.IsUsingUnixDomainSockets = this.TransportOptions.IsUsingUnixDomainSockets;
            this._mockConfig.UnixDomainSockets.SocketFileName = this.TransportOptions.SocketOrPipeName;
        }

        var feature = new SaveServiceMockConfigFeature(this._mockConfigFile, this._mockConfig, this.Io);
        await feature.Save();
        if (nameChanged) {
            this._onServiceMockNameChanged();
        }

        this.Close();
    }

    private Task OnReset() =>
        // GlobalHub.publish(new CloseOverlayMessage(this));
        // var vm = new ResetGrpcServiceOverlayViewModel(this._mockConfigFile);
        // GlobalHub.publish(new OpenOverlayMessage(vm));
        Task.CompletedTask;
}