#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.Features;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryViewModel : GrpCallTypeViewModelBase {
    private string _statusText;
    private UnaryReqViewModel _reqViewModel;
    private bool _showTreeEditor;

    public UnaryViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this._reqViewModel = new UnaryReqViewModel(mi, true);
        this.RespViewModel = new UnaryRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this._statusText = "";
        this._showTreeEditor = true;
        this.ReqViewModel.SubscribeTo(vm => ((UnaryReqViewModel)vm).ShowTreeEditor, OnShowTreeEditorChanged );
    }

    private void OnShowTreeEditorChanged(ViewModelBase obj) {
        this.ReqViewModel = null!;
        this.ReqViewModel = (UnaryReqViewModel)obj;
        //this.RaisePropertyChanged(nameof(this.ReqViewModel));
    }
    public bool ShowTreeEditor {
        get => this._showTreeEditor;
        set {
            this.RaiseAndSetIfChanged(ref _showTreeEditor , value);
            this.ReqViewModel.ShowTreeEditor = value;
            this.RespViewModel.IsShowingResponseTreeEditor = value;
        }
    }
    public UnaryReqViewModel ReqViewModel {
        get => this._reqViewModel;
        private set => this.RaiseAndSetIfChanged(ref _reqViewModel, value);
    }
    public UnaryRespViewModel RespViewModel { get; }
    public ICommand StartCommand { get; }

    public string StatusText {
        get => this._statusText;
        private set => this.RaiseAndSetIfChanged(ref this._statusText, value);
    }

    public override void Dispose() {
        base.Dispose();
        this.ReqViewModel.Dispose();
        this.RespViewModel.Dispose();
    }

    public override void Init() {
        this.ReqViewModel.Init();
    }

    private async Task OnStart() {
        this.IsBusy = true;
        try {
            this.RespViewModel.Init();
            var mi = this.ReqViewModel.MethodInfo;
            var (paramOk, mParams) = this.ReqViewModel.GetMethodParameters();
            if (paramOk) {
                var clientConfig = this.Client.Config.Value;
                var feature = new CallUnaryFeature(mi, mParams, clientConfig, this.Io);
                var (ok, resp) = await feature.Run();
                var (_, response, context) = resp.OkayOrFailed();

                this.StatusText = $"Elapsed {printTimeSpan(context.Elapsed.Value)}";
                this.RespViewModel.Show(ok, response, context);
            }
        }
        finally {
            this.IsBusy = false;
        }
    }

    // public class TestClass {
    //     public byte[] Bytes { get; set; }
    //
    //     public void GetBytes(byte[] bytes, TestEnum[] selections, List<Dictionary<TestClass, TestEnum>> dickinsian) {
    //     }
    // }
    //
    // public enum TestEnum {
    //     A = 1,
    //     B = 2,
    //     C= 3
    // }
}