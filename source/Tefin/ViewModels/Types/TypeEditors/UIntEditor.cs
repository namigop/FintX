namespace Tefin.ViewModels.Types.TypeEditors;

public class UIntEditor(TypeBaseNode node) : TypeEditorBase<uint>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}