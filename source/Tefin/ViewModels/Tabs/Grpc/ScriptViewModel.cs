using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;

using Microsoft.FSharp.Core;
using ReactiveUI;
using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Core.Scripting;
using Tefin.Features;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ScriptViewModel : ViewModelBase {
    private readonly MethodInfo _methodInfo;
    private string _scriptText = "";
    private string _header = "";
    private string _headerTemp = "";
    private bool _isSelected;
    private readonly Type? _serviceType;
    private readonly string _serviceName;
    private Thickness _margin;
    private SolidColorBrush _selectedColor;
    private double _editorHeight;

    public ScriptViewModel(MethodInfo mi, string cgName) {
        this._serviceName = cgName;
        this._methodInfo = mi;
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this.RemoveCommand = this.CreateCommand(this.OnRemove);
        this._serviceType = mi.DeclaringType;

        this.ScriptText = Script.generateDefaultScriptText(mi);
        this.Header = "New Script";
        this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
        this.SubscribeTo( (ScriptViewModel x) => x.IsSelected, OnSelectedScriptChanged);
        this.EditorHeight = 400;
    }

    public ICommand CompileCommand { get; }

    public ICommand RemoveCommand { get; }

    private void OnRemove() {
        this.Scripts.Remove(this);
    }

    

    private void OnSelectedScriptChanged(ScriptViewModel obj) {
        if (obj._isSelected) {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSeaGreen"));
            ServerHandler.Register(obj._serviceName, obj._methodInfo, () => obj._scriptText);
            foreach (var s in Scripts) {
                if (s != obj)
                    s.IsSelected = false;
            }
        }
        else {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
            ServerHandler.UnRegister(obj._serviceName, obj._methodInfo);
        }
    }

    private void OnCompile() {
        var parseResult = ScriptParser.parse.Invoke(this._scriptText);
        if (parseResult.IsOk) {
            var env = ServerHandler.GetOrAddScriptEnv(this._serviceName);
            foreach (var line in parseResult.ResultValue) {
                if (line.ContainsScript) {
                    var code = ScriptParser.extract.Invoke(line.Raw);
                    Io.Log.Info($"Compiling script: {code.Runnable}");
                    Script.getOrAddRunner(env.Engine, code.Runnable, typeof(ServerGlobals));
                    Io.Log.Info($"Done. all good.");
                }
            }
        }
        else {
            Io.Log.Error(parseResult.ErrorValue);
        }
        
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
    public string HeaderTemp {
        get => this._headerTemp;
        set => this.RaiseAndSetIfChanged(ref this._headerTemp, value);
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

    public double EditorHeight {
        get => this._editorHeight;
        set => this.RaiseAndSetIfChanged(ref _editorHeight, value);
    }

    public SingleScript ToScriptFileContent() {
        return new SingleScript(this.IsSelected, this.ScriptText);
    }
}