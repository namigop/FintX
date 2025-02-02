namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableDecimalEditor(TypeBaseNode node) : TypeEditorBase<decimal?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}