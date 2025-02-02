namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableByteEditor(TypeBaseNode node) : TypeEditorBase<byte?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}