#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class TypeInfo : ITypeInfo {
    private readonly FieldInfo? fieldInfo;
    private readonly ParameterInfo? paramInfo;
    private readonly PropertyInfo? propertyInfo;
    private object? paramInstance;

    public TypeInfo(PropertyInfo propInfo) {
        this.propertyInfo = propInfo;
        this.Name = propInfo.Name;
        this.CanRead = propInfo.CanRead;
        this.CanWrite = propInfo.CanWrite;
    }

    public TypeInfo(FieldInfo fieldInfo) {
        this.fieldInfo = fieldInfo;
        this.Name = fieldInfo.Name;
        this.CanRead = fieldInfo.IsPublic;
        this.CanWrite = fieldInfo.IsPublic;
    }

    public TypeInfo(ParameterInfo? paramInfo, object? paramInstance) {
        this.paramInstance = paramInstance;
        this.paramInfo = paramInfo;
        this.Name = paramInfo?.Name ?? "";
        this.CanRead = true;
        this.CanWrite = true;
    }

    public bool CanRead { get; }
    public bool CanWrite { get; }
    public FieldInfo? FieldInfo => this.fieldInfo;
    public string Name { get; }
    public PropertyInfo? PropertyInfo => this.propertyInfo;

    public object? GetValue(object parent) {
        if (this.paramInfo != null)
            return this.paramInstance;

        if (this.fieldInfo != null)
            return this.fieldInfo.GetValue(parent)!;

        return this.propertyInfo!.GetValue(parent);
    }

    public virtual void SetValue(object parentInstance, object value) {
        if (this.paramInfo != null)
            this.paramInstance = value;
        //throw new Exception("Unable to call SetValue(...) on a ParameterInfo");

        if (this.fieldInfo != null && this.fieldInfo.IsPublic)
            this.fieldInfo.SetValue(parentInstance, value);

        if (this.propertyInfo != null && this.propertyInfo.CanWrite)
            this.propertyInfo.SetValue(parentInstance, value);
    }
}