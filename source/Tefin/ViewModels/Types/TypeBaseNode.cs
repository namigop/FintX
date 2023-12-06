#region

using System.Linq;
using System.Reflection;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Types;

public abstract class TypeBaseNode : NodeBase {
    private readonly ITypeInfo? _typeInfo;
    private bool _isNull; 
    private object? _value;

    protected TypeBaseNode(string name, Type type, ITypeInfo? typeInfo, object? instance, TypeBaseNode? parent) {
        this.Title = name;
        this.FormattedTypeName = type.Name;
        this._isNull = instance == null;
        this.CanSetAsNull = TypeHelper.getDefault(type) == null;
        this.SubTitle = "";
        this._typeInfo = typeInfo;
        this._value = instance;
        this.Parent = parent;
        this.Type = type;
    }

    public bool CanSetAsNull { get; }
    public virtual string FormattedTypeName { get; }
    public virtual string FormattedValue => this._value?.ToString() ?? "null";

    public bool IsNull {
        get => this._isNull;
        set => this.RaiseAndSetIfChanged(ref this._isNull, value);
    }

    public TypeBaseNode? Parent { get; }
    public Type Type { get; }

    //  set => this.RaiseAndSetIfChanged(ref this._formattedValue, value);
    public object? Value {
        get => this._value;
        set {
            var oldValue = this._value;
            if (this._value != value) {
                this._value = value;
                this.OnValueChanged(oldValue, this._value);
                this.RaisePropertyChanged(nameof(this.FormattedValue));
            }
        }
    }

    public abstract void Init(Dictionary<string, int> processedTypeNames);

    public override void Init() {
        this.Init(new Dictionary<string, int>());
    }

    protected List<PropertyInfo> GetProperties() {
        var t = this.Type;
        var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead || p.CanWrite)

            //ignore all classes under the System.Reflection namespace
            .Where(p => p.PropertyType.Namespace == null || (p.PropertyType.Namespace != null && !p.PropertyType.Namespace.StartsWith("System.Reflection")))
            .Where(p => p.PropertyType != typeof(IDictionary<string, object>)).OrderBy(p => p.Name).ToList();
        return props;
    }

    protected virtual void OnValueChanged(object? oldValue, object? newValue) {
        var parentInstance = this.Parent?.Value;
        if (parentInstance != null)
            this._typeInfo?.SetValue(parentInstance, newValue);
    }
}