#region

using Google.Protobuf.WellKnownTypes;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class TimestampEditor : TypeEditorBase<Timestamp> {

    public TimestampEditor(TypeBaseNode node) : base(node) {
    }
}