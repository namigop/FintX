namespace Tefin.ViewModels.Types.TypeEditors;

public class Float32Editor(TypeBaseNode node) : TypeEditorBase<float>(node) {
    public SystemNode TypeNode => (SystemNode)this.Node;
}