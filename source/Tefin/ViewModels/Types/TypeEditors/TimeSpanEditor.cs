namespace Tefin.ViewModels.Types.TypeEditors;

public class TimeSpanEditor(TypeBaseNode node) : TypeEditorBase<TimeSpan>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}