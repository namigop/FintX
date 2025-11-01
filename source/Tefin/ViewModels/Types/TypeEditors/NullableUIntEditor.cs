namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableUIntEditor(TypeBaseNode node) : TypeEditorBase<uint?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}