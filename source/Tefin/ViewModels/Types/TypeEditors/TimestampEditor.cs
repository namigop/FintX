#region

using Google.Protobuf.WellKnownTypes;

using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class TimestampEditor(TypeBaseNode node) : TypeEditorBase<Timestamp>(node) {
    public TimestampNode TypeNode => (TimestampNode)this.Node;

    public override string FormattedValue { get => TypeNode.DateTimeText; }
}