#region

using System.Threading;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Types.TypeEditors;

#endregion

namespace Tefin.ViewModels.Types;

public class CancellationTokenNode : TypeBaseNode {
    
    public CancellationTokenNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode parent) : base(name, type, propInfo, instance, parent) {
        this.FormattedTypeName = $"{{{SystemType.getDisplayName(type)}}}";
        this.Editor = new CancellationTokenEditor(this);
    }

    public ITypeEditor Editor { get; }
    public override string FormattedTypeName { get; }
    public override string FormattedValue => (CancellationToken)this.Value! == CancellationToken.None ? "None" : "Token";
    public CancellationTokenSource? Source { get; set; }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        //no child nodes
    }

    protected override void OnValueChanged(object? oldValue, object? newValue) {
    }
}