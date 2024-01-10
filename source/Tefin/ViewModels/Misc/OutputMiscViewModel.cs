namespace Tefin.ViewModels.Misc;

public class OutputMiscViewModel : MiscViewModelTabItem {
    public Editor Editor { get; } = new();
    public override string Title { get; } = "Output";
}