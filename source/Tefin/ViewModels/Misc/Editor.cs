using AvaloniaEdit;

namespace Tefin.ViewModels.Misc;

public class Editor {
    private readonly ConsoleIntercept _ci;

    public Editor() {
        this._ci = new ConsoleIntercept();
        Console.SetOut(this._ci);
    }

    public void SetTarget(TextEditor textEditor) {
        this._ci.TextEditor = textEditor;
    }
}