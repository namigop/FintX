#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class TypeInfo : ITypeInfo {
    private readonly ParameterInfo? _paramInfo;
    private object? _paramInstance;

    public TypeInfo(PropertyInfo propInfo) {
        this.PropertyInfo = propInfo;
        this.Name = propInfo.Name;
        this.CanRead = propInfo.CanRead;
        this.CanWrite = propInfo.CanWrite;
    }

    public TypeInfo(FieldInfo fieldInfo) {
        this.FieldInfo = fieldInfo;
        this.Name = fieldInfo.Name;
        this.CanRead = fieldInfo.IsPublic;
        this.CanWrite = fieldInfo.IsPublic;
    }

    public TypeInfo(ParameterInfo? paramInfo, object? paramInstance) {
        this._paramInstance = paramInstance;
        this._paramInfo = paramInfo;
        this.Name = paramInfo?.Name ?? "";
        this.CanRead = true;
        this.CanWrite = true;
    }

    public bool CanRead { get; }
    public bool CanWrite { get; }

    public FieldInfo? FieldInfo {
        get;
    }

    public string Name { get; }

    public PropertyInfo? PropertyInfo {
        get;
    }

    public object? GetValue(object parent) {
        if (this._paramInfo != null)
            return this._paramInstance;

        if (this.FieldInfo != null)
            return this.FieldInfo.GetValue(parent)!;

        return this.PropertyInfo!.GetValue(parent);
    }

    public virtual void SetValue(object parentInstance, object? value) {
        if (this._paramInfo != null)
            this._paramInstance = value;
        //throw new Exception("Unable to call SetValue(...) on a ParameterInfo");

        if (this.FieldInfo != null && this.FieldInfo.IsPublic)
            this.FieldInfo.SetValue(parentInstance, value);

        if (this.PropertyInfo != null && this.PropertyInfo.CanWrite)
            this.PropertyInfo.SetValue(parentInstance, value);
    }
}