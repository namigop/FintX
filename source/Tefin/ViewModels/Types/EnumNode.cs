#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types;

public class EnumNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent)
    : TypeBaseNode(name, type, propInfo, instance, parent) {
    private object? _selectedEnumValue = instance;

    public Array EnumValues { get; } = Enum.GetValues((type.IsEnum ? type : Nullable.GetUnderlyingType(type))!);

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