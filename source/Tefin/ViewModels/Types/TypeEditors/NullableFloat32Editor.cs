namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableFloat32Editor(TypeBaseNode node) : TypeEditorBase<float?>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}