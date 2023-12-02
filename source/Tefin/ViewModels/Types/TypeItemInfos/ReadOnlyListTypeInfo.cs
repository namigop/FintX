#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class ReadOnlyListTypeInfo : TypeInfo {
    private readonly MethodInfo addRangeMethod;
    private readonly MethodInfo clearMethod;
    private readonly object syncList;

    public ReadOnlyListTypeInfo(PropertyInfo propInfo) : base(propInfo) {
        //we are using this only for Lists
        var type = propInfo.PropertyType;
        this.clearMethod = type.GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
        this.addRangeMethod = type.GetMethod("AddRange", BindingFlags.Public | BindingFlags.Instance);
    }

    public override void SetValue(object parentInstance, object value) {
        var parentListValue = this.PropertyInfo.GetValue(parentInstance);

        //Update the list only if we are working on different instances
        if (parentListValue != value) {
            this.clearMethod.Invoke(parentListValue, null);
            this.addRangeMethod.Invoke(parentListValue, new[] { value });
        }
    }
}