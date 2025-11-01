namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableIntEditor(TypeBaseNode node) : TypeEditorBase<int?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}