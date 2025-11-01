namespace Tefin.ViewModels.Types.TypeEditors;

public class DecimalEditor(TypeBaseNode node) : TypeEditorBase<decimal>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}