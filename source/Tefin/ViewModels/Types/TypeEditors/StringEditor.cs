namespace Tefin.ViewModels.Types.TypeEditors;

public class StringEditor : TypeEditorBase<string> {

    public StringEditor(TypeBaseNode node) : base(node) {
    }

    public override bool AcceptsNull => true;
}