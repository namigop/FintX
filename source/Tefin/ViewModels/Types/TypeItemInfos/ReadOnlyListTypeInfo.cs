#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ReadOnlyListTypeInfo : TypeInfo {
    private readonly MethodInfo _addRangeMethod;
    private readonly MethodInfo _clearMethod;

    public ReadOnlyListTypeInfo(PropertyInfo propInfo) : base(propInfo) {
        //we are using this only for Lists
        var type = propInfo.PropertyType;
        this._clearMethod = type.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance)!;
        this._addRangeMethod = type.GetMethod("AddRange", BindingFlags.Public | BindingFlags.Instance)!;
    }

    public override void SetValue(object parentInstance, object? value) {
        var parentListValue = this.PropertyInfo!.GetValue(parentInstance);

        //Update the list only if we are working on different instances
        if (parentListValue != value) {
            this._clearMethod.Invoke(parentListValue, null);
            this._addRangeMethod.Invoke(parentListValue, new[] { value });
        }
    }
}