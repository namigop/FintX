namespace Tefin.ViewModels.Misc;

public class OutputMiscViewModel : MiscViewModelTabItem {

    public OutputMiscViewModel() {
        this.Editor = new Editor();
    }

    public Editor Editor { get; }
    public override string Title { get; } = "Output";
}