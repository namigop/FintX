namespace Tefin.ViewModels.Types.TypeEditors;

public class GuidEditor(TypeBaseNode node) : TypeEditorBase<Guid>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}