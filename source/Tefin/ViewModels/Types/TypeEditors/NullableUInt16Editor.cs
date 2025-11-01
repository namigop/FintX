namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableUInt16Editor(TypeBaseNode node) : TypeEditorBase<ushort?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}