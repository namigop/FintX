namespace Tefin.ViewModels.Types.TypeEditors;

public class UInt64Editor(TypeBaseNode node) : TypeEditorBase<ulong>(node){
    public SystemNode TypeNode => (SystemNode)this.Node;
}