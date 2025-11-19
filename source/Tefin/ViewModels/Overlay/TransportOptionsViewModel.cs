using System.Windows.Input;

using ReactiveUI;

using Tefin.Utils;
using Tefin.ViewModels.Tabs.Grpc;

namespace Tefin.ViewModels.Overlay;

public class TransportOptionsViewModel : ViewModelBase {
    public const string Default = "Default";
    public const string NamedPipes = "Named Pipes";
    public const string UnixDomainSockets = "Unix Domain Sockets";

    private bool _isUsingNamedPipes;
    private string _socketOrPipeName;
    private bool _isUsingUnixDomainSockets;
    private string _selectedTransport;
    private bool _isSocketOrNamedPipe;
    private string _socketOrPipeNameWatermark;

    public TransportOptionsViewModel() {

        if (Core.Utils.isWindows())
            this.Transports = new List<string> { Default, NamedPipes, UnixDomainSockets };
        else
            this.Transports = new List<string> { Default, UnixDomainSockets };

        this.SelectedTransport = "Default";
        this.SelectSocketFileCommand = this.CreateCommand(OnSelectSocketFile);
        this.SubscribeTo<string, TransportOptionsViewModel>(
            x => x.SelectedTransport,
            vm => {
                vm.IsUsingNamedPipes = vm.SelectedTransport == NamedPipes;
                vm.IsUsingUnixDomainSockets = vm.SelectedTransport == UnixDomainSockets;
                vm.IsSocketOrNamedPipe = vm.IsUsingNamedPipes || vm.IsUsingUnixDomainSockets;
                if (vm.IsUsingUnixDomainSockets)
                    vm.SocketOrPipeNameWatermark = "Enter path to the socket file";
                if (vm.IsUsingNamedPipes)
                    vm.SocketOrPipeNameWatermark = "Enter the pipe name";
            });
    }

    private async Task OnSelectSocketFile() {
        var fileTitle = "Socket file";
         
        var (ok, files) = await DialogUtils.OpenFile("Select socket file", fileTitle, ["*.*"]);
        if (ok) {
            this.SocketOrPipeName = files[0];
        }
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

    public string SocketOrPipeName {
        get => this._socketOrPipeName;
        set => this.RaiseAndSetIfChanged(ref this._socketOrPipeName, value);
    }

    public string SelectedTransport {
        get => this._selectedTransport;
        set => this.RaiseAndSetIfChanged(ref this._selectedTransport, value);
    }
    public bool IsSocketOrNamedPipe {
        get => this._isSocketOrNamedPipe;
        private set => this.RaiseAndSetIfChanged(ref this._isSocketOrNamedPipe, value);
    }

    public string SocketOrPipeNameWatermark {
        get => this._socketOrPipeNameWatermark;
        private set => this.RaiseAndSetIfChanged(ref this._socketOrPipeNameWatermark, value);
    }
    public List<string> Transports { get; private set; }
    public ICommand SelectSocketFileCommand { get; }
}