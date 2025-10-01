using System.Collections.ObjectModel;
using System.Reflection;
//using System.Reflection.Emit;
using System.Windows.Input;

using Tefin.Core.Interop;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    private readonly MethodInfo _mi;

    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg) {
        this._mi = mi;
        this.Scripts = new ObservableCollection<ScriptViewModel>();
        var scm = new ScriptViewModel(mi, cg.Name) { Scripts = this.Scripts};
        this.Scripts.Add(scm);

        this.AddScriptCommand = CreateCommand(OnAddScript);
    }

    public ICommand AddScriptCommand { get; }

    private void OnAddScript() {
        this.Scripts.Add(new ScriptViewModel(this._mi, this.ServiceMock.Name){ Scripts = this.Scripts});
    }

    public ObservableCollection<ScriptViewModel> Scripts { get; }

    public override bool IsLoaded { get => this.Scripts.Count > 0; }

   
    public override string GetScriptContent() => "<script>";

    public override void Init() {
        
    }
}