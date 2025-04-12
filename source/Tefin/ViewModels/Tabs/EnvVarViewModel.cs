using DynamicData;

using Google.Protobuf.WellKnownTypes;

using Microsoft.FSharp.Control;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Utils;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeEditors;

using Type = System.Type;

namespace Tefin.ViewModels.Tabs;

public class EnvVarViewModel : ViewModelBase {
    private string _description = "";
    private string _type = "";
    private string _defaultValue = "";
    private string _currentValue = "";
    private string _name = "";
    private string _selectedDisplayType;
    private static string[] DisplayTypes = SystemType.getTypesForDisplay();
    private static Type[] ActualTypes = SystemType.getTypes().ToArray();
    
    public EnvVarViewModel(EnvVar envVar) {
        this.Name = envVar.Name;
        this.CurrentValue = envVar.CurrentValue;
        this.DefaultValue = envVar.DefaultValue;
        this.Description = envVar.Description;
        this.DisplayType = envVar.Type;
        var actualType = DisplayTypes.IndexOf(envVar.Type).Then(i => ActualTypes[i]);
        
        var cur = TypeHelper.indirectCast(envVar.CurrentValue, actualType);
        var def = TypeHelper.indirectCast(envVar.DefaultValue, actualType);
        
        var currentValueNode = new SystemNode(envVar.Name, actualType, default, cur, null);
        this.CurrentValueEditor = currentValueNode.Editor;
        var defaultValueNode =new SystemNode(envVar.Name, actualType, default, def, null);
        this.DefaultValueEditor = defaultValueNode.Editor;
        this._selectedDisplayType = this.DisplayType;
    }

    public string[] TypeList => DisplayTypes;

    public string SelectedDisplayType {
        get => this._selectedDisplayType;
        set => this.RaiseAndSetIfChanged(ref _selectedDisplayType, value);
    }

    public ITypeEditor DefaultValueEditor { get; init; }

    public ITypeEditor CurrentValueEditor { get; init; }

    public string DisplayType {
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