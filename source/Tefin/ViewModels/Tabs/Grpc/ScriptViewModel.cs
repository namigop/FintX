using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

using Avalonia;
using Avalonia.Media;

using ReactiveUI;

using Tefin.Core.Scripting;
using Tefin.Features.Scripting;

namespace Tefin.ViewModels.Tabs.Grpc;

public class ScriptViewModel : ViewModelBase {
    private readonly Type? _serviceType;
    private double _editorHeight;
    private string _header = "";
    private string _headerTemp = "";
    private bool _isSelected;
    private Thickness _margin;
    private string _scriptText = "";
    private SolidColorBrush _selectedColor;

    public ScriptViewModel(MethodInfo mi, string serviceName, Action<ScriptViewModel> onRemove) {
        this.ServiceName = serviceName;
        this.MethodInfo = mi;
        this.CompileCommand = this.CreateCommand(this.OnCompile);
        this.RemoveCommand = this.CreateCommand(() => onRemove(this));
        this._serviceType = mi.DeclaringType;

        this.ScriptText = Script.generateDefaultScriptText(mi);
        this.Header = "New Script";
        this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
        this.SubscribeTo((ScriptViewModel x) => x.IsSelected, this.OnSelectedScriptChanged);
        this.EditorHeight = 400;
    }

    public ICommand CompileCommand { get; }

    public double EditorHeight {
        get => this._editorHeight;
        set => this.RaiseAndSetIfChanged(ref this._editorHeight, value);
    }

    public string Header {
        get => this._header;
        set => this.RaiseAndSetIfChanged(ref this._header, value);
    }

    public string HeaderTemp {
        get => this._headerTemp;
        set => this.RaiseAndSetIfChanged(ref this._headerTemp, value);
    }

    public bool IsSelected {
        get => this._isSelected;
        set => this.RaiseAndSetIfChanged(ref this._isSelected, value);
    }

    public Thickness Margin {
        get => this._margin;
        private set => this.RaiseAndSetIfChanged(ref this._margin, value);
    }

    public MethodInfo MethodInfo { get; }

    public ICommand RemoveCommand { get; }

    public required ObservableCollection<ScriptViewModel> Scripts { get; init; }

    public string ScriptText {
        get => this._scriptText;
        set => this.RaiseAndSetIfChanged(ref this._scriptText, value);
    }

    public SolidColorBrush SelectedColor {
        get => this._selectedColor;
        private set => this.RaiseAndSetIfChanged(ref this._selectedColor, value);
    }

    public string ServiceName { get; }

    private void OnCompile() {
        var parseResult = ScriptParser.parse.Invoke(this._scriptText);
        if (parseResult.IsOk) {
            var env = ServerHandler.GetOrAddScriptEnv(this.ServiceName);
            foreach (var line in parseResult.ResultValue) {
                if (line.ContainsScript) {
                    var code = ScriptParser.extract.Invoke(line.Raw);
                    this.Io.Log.Info($"Compiling script: {code.Runnable}");
                    Script.getOrAddRunner(env.Engine, code.Runnable, typeof(ServerGlobals));
                    this.Io.Log.Info("Done. all good.");
                }
            }
        }
        else {
            this.Io.Log.Error(parseResult.ErrorValue);
        }
    }

    private void OnSelectedScriptChanged(ScriptViewModel obj) => obj.TryRegisterMethod();

    public SingleScript ToScriptFileContent() => new(this.IsSelected, this.ScriptText);

    public void TryRegisterMethod() {
        if (this._isSelected) {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSeaGreen"));
            ServerHandler.UnRegister(this.ServiceName, this.MethodInfo);
            ;
            ServerHandler.Register(this.ServiceName, this.MethodInfo, () => this.ScriptText);
            foreach (var s in this.Scripts) {
                if (s != this) {
                    s.IsSelected = false;
                }
            }

            ServerHandler.Register(this.ServiceName, this.MethodInfo, () => this.ScriptText);
        }
        else {
            this.SelectedColor = new SolidColorBrush(Color.Parse("LightSlateGray"));
            ServerHandler.UnRegister(this.ServiceName, this.MethodInfo);
        }
    }
}