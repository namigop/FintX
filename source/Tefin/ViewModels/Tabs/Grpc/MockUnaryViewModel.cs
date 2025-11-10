using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Scripting;
using Tefin.Features.Scripting;

using File = System.IO.File;

//using System.Reflection.Emit;

namespace Tefin.ViewModels.Tabs.Grpc;

public class MockUnaryViewModel : GrpMockCallTypeViewModelBase {
    private readonly MethodInfo _mi;

    public MockUnaryViewModel(MethodInfo mi, ProjectTypes.ServiceMockGroup cg) : base(mi, cg) {
        this._mi = mi;
        this.Scripts = new ObservableCollection<ScriptViewModel>();
        var scm = new ScriptViewModel(mi, cg.Name, this.OnRemoveScript) { Scripts = this.Scripts };
        scm.IsSelected = true;
        this.Scripts.Add(scm);
        this.AddScriptCommand = this.CreateCommand(this.OnAddScript);
        //this.RemoveScriptCommand = this.CreateCommand(this.OnRemoveScript);
    }

    public ICommand AddScriptCommand { get; }

    public override bool IsLoaded => this.Scripts.Count > 0;

    public ObservableCollection<ScriptViewModel> Scripts { get; }

    public override void Dispose() => ServerHandler.Dispose(this.ServiceMock.Name);


    public override string GetScriptContent() {
        var contents = this.Scripts.Select(v => v.ToScriptFileContent()).ToArray();
        var m = new ScriptFile(this._mi.DeclaringType!.Name, this._mi.Name, contents);
        return Instance.jsonSerialize(m);
    }

    public override void ImportScript(string scriptFile) {
        if (!File.Exists(scriptFile)) {
            return;
        }

        var content = this.Io.File.ReadAllText(scriptFile);
        var m = Instance.jsonDeserialize<ScriptFile>(content);
        this.Scripts.Clear();
        foreach (var s in m.Scripts) {
            var scm = new ScriptViewModel(this._mi, this.ServiceMock.Name, this.OnRemoveScript) {
                Scripts = this.Scripts
            };
            scm.ScriptText = s.Content;
            scm.IsSelected = s.IsSelected;
            this.Scripts.Add(scm);
            scm.TryRegisterMethod();
        }

        if (this.Scripts.Any()) {
            var hasSelected = this.Scripts.Any(v => v.IsSelected);
            if (!hasSelected) {
                this.Scripts.First().IsSelected = true;
            }
        }

        this.SetEditorHeight();
    }


    public override void Init() {
    }

    private void OnAddScript() {
        this.Scripts.Add(
            new ScriptViewModel(this._mi, this.ServiceMock.Name, this.OnRemoveScript) { Scripts = this.Scripts });
        this.SetEditorHeight();
    }

    private void OnRemoveScript(ScriptViewModel vm) {
        this.Scripts.Remove(vm);
        if (vm.IsSelected && this.Scripts.Any()) {
            this.Scripts.First().IsSelected = true;
        }

        this.SetEditorHeight();
    }

    private void SetEditorHeight() {
        var height = this.Scripts.Count == 1 ? 600 : 400;
        foreach (var s in this.Scripts) {
            s.EditorHeight = height;
        }
    }
}