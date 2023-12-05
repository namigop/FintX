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

    public UnaryViewModel(MethodInfo mi, ProjectTypes.ClientGroup cg) : base(mi, cg) {
        this.ReqViewModel = new UnaryReqViewModel(mi, true);
        this.RespViewModel = new UnaryRespViewModel(mi);
        this.StartCommand = this.CreateCommand(this.OnStart);
        this.StatusText = "";
    }

    public UnaryReqViewModel ReqViewModel { get; set; }
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
            var mParams = this.ReqViewModel.GetMethodParameters();
            var clientConfig = this.Client.Config.Value;

            var feature = new CallUnaryFeature(mi, mParams, clientConfig, this.Io);
            var (ok, resp) = await feature.Run();
            var (_, response, context) = resp.OkayOrFailed();

            this.StatusText = $"Elapsed {printTimeSpan(context.Elapsed.Value)}";
            this.RespViewModel.Show(ok, response, context);
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