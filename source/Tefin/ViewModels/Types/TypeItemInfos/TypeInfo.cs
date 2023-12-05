#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public class TypeInfo : ITypeInfo {
    private readonly FieldInfo? _fieldInfo;
    private readonly ParameterInfo? _paramInfo;
    private readonly PropertyInfo? _propertyInfo;
    private object? _paramInstance;

    public TypeInfo(PropertyInfo propInfo) {
        this._propertyInfo = propInfo;
        this.Name = propInfo.Name;
        this.CanRead = propInfo.CanRead;
        this.CanWrite = propInfo.CanWrite;
    }

    public TypeInfo(FieldInfo fieldInfo) {
        this._fieldInfo = fieldInfo;
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
    public FieldInfo? FieldInfo => this._fieldInfo;
    public string Name { get; }
    public PropertyInfo? PropertyInfo => this._propertyInfo;

    public object? GetValue(object parent) {
        if (this._paramInfo != null)
            return this._paramInstance;

        if (this._fieldInfo != null)
            return this._fieldInfo.GetValue(parent)!;

        return this._propertyInfo!.GetValue(parent);
    }

    public virtual void SetValue(object parentInstance, object value) {
        if (this._paramInfo != null)
            this._paramInstance = value;
        //throw new Exception("Unable to call SetValue(...) on a ParameterInfo");

        if (this._fieldInfo != null && this._fieldInfo.IsPublic)
            this._fieldInfo.SetValue(parentInstance, value);

        if (this._propertyInfo != null && this._propertyInfo.CanWrite)
            this._propertyInfo.SetValue(parentInstance, value);
    }
}