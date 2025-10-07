using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core.Interop;
using Tefin.Features;

//using System.Reflection.Emit;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    private readonly MethodInfo _mi;

    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg) {
        this._mi = mi;
        this.Scripts = new ObservableCollection<ScriptViewModel>();
        var scm = new ScriptViewModel(mi, cg.Name) { Scripts = this.Scripts };
        this.Scripts.Add(scm);
        this.AddScriptCommand = this.CreateCommand(this.OnAddScript);
    }

    public ICommand AddScriptCommand { get; }

    public ObservableCollection<ScriptViewModel> Scripts { get; }

    public override bool IsLoaded => this.Scripts.Count > 0;

    private void OnAddScript() =>
        this.Scripts.Add(new ScriptViewModel(this._mi, this.ServiceMock.Name) { Scripts = this.Scripts });


    public override string GetScriptContent() => "<script>";

    public override void Init() {
    }

    public override void Dispose() {
        ServerHandler.Dispose(this.ServiceMock.Name);
    }
}