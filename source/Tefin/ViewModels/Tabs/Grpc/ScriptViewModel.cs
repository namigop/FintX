using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using Avalonia;
using Avalonia.Media;

using Microsoft.FSharp.Core;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Features;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ScriptViewModel : ViewModelBase {
    private readonly MethodInfo _methodInfo;
    private string _scriptText = "";
    private string _header = "";
    private bool _isSelected;
    private readonly Type? _serviceType;
    private readonly string _serviceName;
    private Thickness _margin;
    private SolidColorBrush _selectedColor;

    public ScriptViewModel(MethodInfo mi, string cgName) {
        this._serviceName = cgName;
        this._methodInfo = mi;
        
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this._serviceType = mi.DeclaringType;

        var (created, resp) = TypeBuilder.getDefault(GetReturnType(mi.ReturnType), true, FSharpOption<object>.None, 0);
        var json = Instance.indirectSerialize(GetReturnType(mi.ReturnType), resp);
        this._scriptText = json;
        this._header = "New Script";

        this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
        Type GetReturnType(Type retType) {
            if (retType.IsGenericType && retType.GetGenericTypeDefinition() == typeof(Task<>)) {
                return retType.GetGenericArguments()[0];
            }
            return retType;
        }
        
        this.SubscribeTo( (ScriptViewModel x) => x.IsSelected, OnSelectedScriptChanged);
    }

    private void OnSelectedScriptChanged(ScriptViewModel obj) {
        if (obj._isSelected) {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSeaGreen"));
            ServerHandler.Register(obj._serviceName, obj._methodInfo, obj._scriptText);
            foreach (var s in Scripts) {
                if (s != obj)
                    s.IsSelected = false;
            }
        }
        else {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
            ServerHandler.UnRegister(obj._serviceName, obj._methodInfo, obj._scriptText);
        }
    }

    public ICommand CompileCommand { get; }

    private Task OnCompile() {
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

    public Thickness Margin {
        get => this._margin;
        private set => this.RaiseAndSetIfChanged(ref _margin, value);
    }

    public required ObservableCollection<ScriptViewModel> Scripts { get; init; }

    public SolidColorBrush SelectedColor {
        get => this._selectedColor;
        private set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
    }
}