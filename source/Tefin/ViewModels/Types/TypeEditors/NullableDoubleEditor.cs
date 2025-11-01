namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableDoubleEditor(TypeBaseNode node) : TypeEditorBase<double?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}