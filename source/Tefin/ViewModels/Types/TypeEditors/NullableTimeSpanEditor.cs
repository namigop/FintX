namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableTimeSpanEditor(TypeBaseNode node) : TypeEditorBase<TimeSpan?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}