namespace Tefin.ViewModels.Types.TypeEditors;

public class DoubleEditor(TypeBaseNode node) : TypeEditorBase<double>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}