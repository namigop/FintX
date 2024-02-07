#region

using System.Diagnostics;
using System.Reflection;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Types.TypeNodeBuilders;

#endregion

namespace Tefin.ViewModels.Types;

public class DefaultNode : TypeBaseNode {
    private bool _valueIsNull;

    public DefaultNode(string name, Type type, ITypeInfo? propInfo, object? instance, TypeBaseNode? parent) : base(name,
        type, propInfo, instance, parent) {
        this.FormattedTypeName = type.FullName!.StartsWith("Tefin") ? "" : $"{{{type.Name}}}";
        this.IsExpanded = true;
        this.SubscribeTo(x => ((DefaultNode)x).ValueIsNull, this.OnValueIsNullChangedl);
        this._valueIsNull = this.IsNull;
    }

    public override string FormattedTypeName { get; }

    public override string FormattedValue {
        get {
            if (this.IsNull) {
                return "null";
            }

            return "";
        }
    }

    public bool ValueIsNull {
        get => this._valueIsNull;
        set => this.RaiseAndSetIfChanged(ref this._valueIsNull, value);
    }

    public static bool CanCreateChildNodes(Type parameterType, Dictionary<string, int> processedTypeNames,
        object? instance) {
        if (instance == null) {
            return false;
        }

        if (SystemType.isSystemType(parameterType)) {
            return false;
        }

        var instanceCountLimit = 100000; //TODO:Count is configurable
        if (processedTypeNames.TryGetValue(parameterType.FullName!, out var count) && count == instanceCountLimit) {
            return false;
        }

        processedTypeNames[parameterType.FullName!] = count == 0 ? 1 : count + 1;
        return true;
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        if (this.Items.Count > 0) {
            return; //already initialized
        }

        Debug.Assert(this.Items?.Count == 0, "2nd initialize?");

        //if (this.GeneratePropertyNodes && CanCreateChildNodes(this.Type, processedTypeNames, instance)) {
        if (CanCreateChildNodes(this.Type, processedTypeNames, this.Value)) {
            var props = this.GetProperties();
            foreach (var propInfo in props) {
                var propInstance = propInfo.GetValue(this.Value);
                var type = propInstance?.GetType() ?? propInfo.PropertyType;
                var node = TypeNodeBuilder.Create(propInfo.Name, type, new TypeInfo(propInfo), processedTypeNames,
                    propInstance, this);

                this.Items.Add(node);
                node.Init();
            }

            var fields = this.Type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(p => p.IsPublic)
                .ToList();
            foreach (var fieldInfo in fields) {
                try {
                    var fieldInstance = fieldInfo.GetValue(this.Value);
                    var type = fieldInstance?.GetType() ?? fieldInfo.FieldType;
                    var node = TypeNodeBuilder.Create(fieldInfo.Name, type, new TypeInfo(fieldInfo), processedTypeNames,
                        fieldInstance, this);
                    node.Init();
                    this.Items.Add(node);
                }
                catch (Exception exc) {
                    this.Io.Log.Error(exc);
                    Debugger.Break();
                }
            }
        }

        this.IsNull = this.Value == null;
        this.ValueIsNull = this.IsNull;
    }

    protected override void OnValueChanged(object? oldValue, object? newValue) {
        this.Items.Clear();
        this.Init();
    }

    private void OnValueIsNullChangedl(ViewModelBase obj) {
        var node = (DefaultNode)obj;
        if (node.ValueIsNull) {
            this.Value = null;
        }
        else {
            var n = Core.Utils.none<object?>();
            var parent = this.Parent as TypeBaseNode;
            if (parent?.Value != null) {
                n = Core.Utils.some(parent?.Value);
            }

            var (__, v) = TypeBuilder.getDefault(this.Type, true, n, 0);
            this.Value = v;
        }
    }
}