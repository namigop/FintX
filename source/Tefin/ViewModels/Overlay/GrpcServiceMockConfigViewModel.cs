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
    private bool _isUsingNamedPipes;
    private string _pipeName;
    private bool _isUsingUnixDomainSockets;
    private string _socketFileName;
    
    public GrpcServiceMockConfigViewModel(string mockConfigFile, Action onServiceMockNameChanged) {
        this._mockConfigFile = mockConfigFile;
        this._onServiceMockNameChanged = onServiceMockNameChanged;
        this.CancelCommand = this.CreateCommand(this.Close);
        this.OkayCommand = this.CreateCommand(this.OnOkay);
        this.ResetCommand = this.CreateCommand(this.OnReset);
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

    public string Url {
        get => this._url;
        set => this.RaiseAndSetIfChanged(ref this._url, value);
    }


    public string Title { get; } = "Edit mock service configuration";

    public bool IsUsingNamedPipes {
        get => this._isUsingNamedPipes;
        set => this.RaiseAndSetIfChanged(ref _isUsingNamedPipes, value);
    }
    public bool IsUsingUnixDomainSockets {
        get => this._isUsingUnixDomainSockets;
        set => this.RaiseAndSetIfChanged(ref _isUsingUnixDomainSockets, value);
    }

    public string PipeName {
        get => this._pipeName;
        set => this.RaiseAndSetIfChanged(ref _pipeName, value);
    }
    public string SocketFileName {
        get => this._socketFileName;
        set => this.RaiseAndSetIfChanged(ref this._socketFileName, value);
    }

    public void Close() => GlobalHub.publish(new CloseOverlayMessage(this));

    private void Load(string mockConfigFile) {
        this._mockConfig = new ReadServiceMockConfigFeature(mockConfigFile, this.Io).Read();
        this.ServiceMockName = this._mockConfig.ServiceName;
        this.Description = this._mockConfig.Desc;
        this.Port = this._mockConfig.Port;
        this.IsUsingNamedPipes = this._mockConfig.IsUsingNamedPipes;
        this.PipeName = this._mockConfig.NamedPipe.PipeName;
        this.IsUsingUnixDomainSockets = this._mockConfig.IsUsingUnixDomainSockets;
        this.SocketFileName = this._mockConfig.UnixDomainSockets.SocketFileName;
    }

    private async Task OnOkay() {
        var nameChanged = this._mockConfig.ServiceName != this.ServiceMockName;
        this._mockConfig.ServiceName = this.ServiceMockName;
        this._mockConfig.Desc = this.Description;
        this._mockConfig.Port = this.Port;
        this._mockConfig.IsUsingNamedPipes = this.IsUsingNamedPipes;
        this._mockConfig.NamedPipe.PipeName = this.PipeName;
        this._mockConfig.IsUsingUnixDomainSockets = this.IsUsingUnixDomainSockets;
        this._mockConfig.UnixDomainSockets.SocketFileName = this.SocketFileName;
        
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