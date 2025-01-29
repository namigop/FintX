using ReactiveUI;

using Tefin.Core;

namespace Tefin.ViewModels.Tabs;

public class EnvVarViewModel : ViewModelBase {
    private string _description = "";
    private string _type = "";
    private string _defaultValue = "";
    private string _currentValue = "";
    private string _name = "";

    public EnvVarViewModel(EnvVar envVar) {
        this.Name = envVar.Name;
        this.CurrentValue = envVar.CurrentValue;
        this.DefaultValue = envVar.DefaultValue;
        this.Description = envVar.Description;
        this.Type = envVar.Type;

    }

    public string Type {
        get => this._type;
        set => this.RaiseAndSetIfChanged(ref this._type, value);
    }

    public string Description {
        get => this._description;
        set => this.RaiseAndSetIfChanged(ref this._description , value);
    }

    public string DefaultValue {
        get => this._defaultValue;
        set => this.RaiseAndSetIfChanged(ref this._defaultValue , value);
    }

    public string CurrentValue {
        get => this._currentValue;
        set => this.RaiseAndSetIfChanged(ref this._currentValue, value);
    }

    public string Name {
        get => this._name;
        set => this.RaiseAndSetIfChanged(ref this._name, value);
    }
}