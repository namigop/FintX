namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableBoolEditor(TypeBaseNode node) : TypeEditorBase<bool?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}