using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeEditors;

namespace Tefin.ViewModels.Tabs;

public class EnvVarViewModel : ViewModelBase {
    private string _description = "";
    private string _type = "";
    private string _defaultValue = "";
    private string _currentValue = "";
    private string _name = "";
    private string _selectedDisplayType;
    private static string[] DisplayTypes = SystemType.getTypesForDisplay();
    public EnvVarViewModel(EnvVar envVar) {
        this.Name = envVar.Name;
        this.CurrentValue = envVar.CurrentValue;
        this.DefaultValue = envVar.DefaultValue;
        this.Description = envVar.Description;
        this.Type = envVar.Type;
        var currentValueNode = new SystemNode(envVar.Name, typeof(int), default, 1, null);
        this.CurrentValueEditor = currentValueNode.Editor;
        var defaultValueNode = new SystemNode(envVar.Name, typeof(int), default, 1, null);
        this.DefaultValueEditor = defaultValueNode.Editor;
        this.SelectedDisplayType = this.Type;
    }

    public string[] TypeList => DisplayTypes;

    public string SelectedDisplayType {
        get => this._selectedDisplayType;
        set => this.RaiseAndSetIfChanged(ref _selectedDisplayType, value);
    }

    public ITypeEditor DefaultValueEditor { get; init; }

    public ITypeEditor CurrentValueEditor { get; init; }

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