namespace Tefin.ViewModels.Types.TypeEditors;

public class IntEditor(TypeBaseNode node) : TypeEditorBase<int>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}