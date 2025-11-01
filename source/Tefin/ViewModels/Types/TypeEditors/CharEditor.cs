namespace Tefin.ViewModels.Types.TypeEditors;

public class CharEditor(TypeBaseNode node) : TypeEditorBase<char>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}