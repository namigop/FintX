namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableInt16Editor(TypeBaseNode node) : TypeEditorBase<short?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}