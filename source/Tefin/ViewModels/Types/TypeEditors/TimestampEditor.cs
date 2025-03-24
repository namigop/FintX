#region

using Google.Protobuf.WellKnownTypes;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class TimestampEditor(TypeBaseNode node) : TypeEditorBase<Timestamp>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}