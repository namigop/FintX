namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableInt64Editor(TypeBaseNode node) : TypeEditorBase<long?>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}