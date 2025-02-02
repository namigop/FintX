namespace Tefin.ViewModels.Types.TypeEditors;

public class BoolEditor(TypeBaseNode node) : TypeEditorBase<bool>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}