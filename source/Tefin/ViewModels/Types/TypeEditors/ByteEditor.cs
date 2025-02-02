namespace Tefin.ViewModels.Types.TypeEditors;

public class ByteEditor(TypeBaseNode node) : TypeEditorBase<byte>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}