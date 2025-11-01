namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableUriEditor(TypeBaseNode node) : TypeEditorBase<Uri?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}