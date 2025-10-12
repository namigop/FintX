using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Scripting;
using Tefin.Features;

using File = System.IO.File;

//using System.Reflection.Emit;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    private readonly MethodInfo _mi;

    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg) {
        this._mi = mi;
        this.Scripts = new ObservableCollection<ScriptViewModel>();
        var scm = new ScriptViewModel(mi, cg.Name) { Scripts = this.Scripts };
        scm.IsSelected = true;
        this.Scripts.Add(scm);
        this.AddScriptCommand = this.CreateCommand(this.OnAddScript);
    }

    public ICommand AddScriptCommand { get; }

    public ObservableCollection<ScriptViewModel> Scripts { get; }

    public override bool IsLoaded => this.Scripts.Count > 0;

    private void OnAddScript() =>
        this.Scripts.Add(new ScriptViewModel(this._mi, this.ServiceMock.Name) { Scripts = this.Scripts });


    public override string GetScriptContent() {
        var contents = this.Scripts.Select(v => v.ToScriptFileContent()).ToArray();
        var m = new ScriptFile(this._mi.DeclaringType!.Name, this._mi.Name, contents);
        return Instance.jsonSerialize(m);
    }

    
    public override void Init() {
    }

    public override void Dispose() {
        ServerHandler.Dispose(this.ServiceMock.Name);
    }

    public override void ImportScript(string scriptFile) {
        if (!File.Exists(scriptFile)) {
            return;
        }
        
        var content = Io.File.ReadAllText(scriptFile);
        var m = Instance.jsonDeserialize<ScriptFile>(content);
        this.Scripts.Clear();
        foreach (var s in m.Scripts) {
            var scm = new ScriptViewModel(this._mi, this.ServiceMock.Name) { Scripts = this.Scripts };
            scm.ScriptText = s.Content;
            scm.IsSelected = s.IsSelected;
            this.Scripts.Add(scm);
        }
    }
}