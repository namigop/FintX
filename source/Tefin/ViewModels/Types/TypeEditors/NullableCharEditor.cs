namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableCharEditor(TypeBaseNode node) : TypeEditorBase<char?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}