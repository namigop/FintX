using ReactiveUI;

namespace Tefin.ViewModels.Types;

public class EnumNode : TypeBaseNode {
    private object? _selectedEnumValue;

    public EnumNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(name, type, propInfo, instance, parent) {
        this.EnumValues = Enum.GetValues((type.IsEnum ? type : Nullable.GetUnderlyingType(type))!);
        this._selectedEnumValue = instance;
    }

    public Array EnumValues { get; }

    public object? SelectedEnumValue {
        get => this._selectedEnumValue;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedEnumValue, value);
            this.Value = this._selectedEnumValue;
            // if (value != selectedEnumValue) {
            //     selectedEnumValue = value;
            //     //this.Value2 = selectedEnumValue;
            //     RaisePropertyChanged(nameof(SelectedEnumValue));
            // }
        }
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
    }
}