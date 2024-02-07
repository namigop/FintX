#region

using AvaloniaEdit;

#endregion

namespace Tefin.ViewModels.Misc;

public class Editor {
    private readonly ConsoleIntercept _ci;

    public Editor() {
        this._ci = new ConsoleIntercept();
        Console.SetOut(this._ci);
    }

    public void Clear() => this._ci.Clear();

    public void SetTarget(TextEditor textEditor) => this._ci.TextEditor = textEditor;
}