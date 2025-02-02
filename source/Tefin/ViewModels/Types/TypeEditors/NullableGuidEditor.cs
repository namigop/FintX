namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableGuidEditor(TypeBaseNode node) : TypeEditorBase<Guid?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}