using System.Reflection;
using System.Windows.Input;

using Microsoft.FSharp.Core;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ScriptViewModel : ViewModelBase {
    private readonly MethodInfo _methodInfo;
    private string _scriptText = "";
    private string _header = "";
    private bool _isSelected;
    private readonly Type? _serviceType;

    public ScriptViewModel(MethodInfo mi) {
        this._methodInfo = mi;
        this.SelectCommand = this.CreateCommand(this.OnSelect);
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this._serviceType = mi.DeclaringType;

        var (created, resp) = TypeBuilder.getDefault(GetReturnType(mi.ReturnType), true, FSharpOption<object>.None, 0);
        var json = Instance.indirectSerialize(GetReturnType(mi.ReturnType), resp);
        this._scriptText = json;
        this._header = "New Script";

        Type GetReturnType(Type retType) {
            if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Task<>)) {
                return retType.GetGenericArguments()[0];
            }
            return retType;
        }
    }

    public ICommand SelectCommand { get; }
    public ICommand CompileCommand { get; }

    private Task OnCompile() {
        throw new NotImplementedException();
    }

    private void OnSelect() {
        throw new NotImplementedException();
    }

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }

    public string ScriptText {
        get => this._scriptText;
        set => this.RaiseAndSetIfChanged(ref this._scriptText, value);
    }

    public string Header {
        get => this._header;
        set => this.RaiseAndSetIfChanged(ref this._header, value);
    }
}