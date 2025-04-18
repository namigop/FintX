namespace Tefin.ViewModels.Types.TypeEditors;

public class StringEditor(TypeBaseNode node) : TypeEditorBase<string>(node) {
    public override bool AcceptsNull => true;
 
    public SystemNode TypeNode => (SystemNode)this.Node;

}