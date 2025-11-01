namespace Tefin.ViewModels.Types.TypeEditors;

public class UriEditor(TypeBaseNode node) : TypeEditorBase<Uri>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}