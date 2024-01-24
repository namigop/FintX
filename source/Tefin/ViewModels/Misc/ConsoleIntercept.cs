#region

using System.Text;

using Avalonia.Threading;

using AvaloniaEdit;

#endregion

namespace Tefin.ViewModels.Misc;

public class ConsoleIntercept : TextWriter {
    private const int Capacity = 1_000_000; //about 4 MB
    private readonly StringBuilder _sb = new();
    private TextEditor? _txtEditor;

    public override Encoding Encoding { get; } = Encoding.UTF8;

    public TextEditor? TextEditor {
        get => this._txtEditor;
        set {
            if (value != null) {
                this._txtEditor = value;
                this._txtEditor.AppendText(this._sb.ToString());
                this.UpdateScroll();
            }
        }
    }

    public void Clear() {
        this._sb.Clear();
        this.Sync();
    }

    public override void Write(StringBuilder? value) {
        this._sb.Append(value);
        this.Sync();
    }

    public override void Write(char value) {
        if (value == (char)27) {
            //ESC char
            this._sb.Append(" ");
            return;
        }

        this._sb.Append(value);
        this.Sync();
    }

    public override void WriteLine(string? value) {
        if (string.IsNullOrEmpty(value))
            return;

        this._sb.AppendLine(value);
        this.Sync();
    }

    private void Sync() {
        if (this.TextEditor == null)
            return;

        Dispatcher.UIThread.Post(() => {
            lock (Console.Out) {
                if (this._sb.Length > this.TextEditor.Document.TextLength) {
                    var startIndex = this.TextEditor.Document.TextLength;
                    var length = this._sb.Length - this.TextEditor.Document.TextLength;
                    var fragment = this._sb.ToString(startIndex, length);
                    this.TextEditor.AppendText(fragment);
                    this.UpdateScroll();
                }

                if (this._sb.Length > Capacity) {
                    var target = this._sb.Length - Capacity;
                    this._sb.Remove(0, target);
                    this.TextEditor!.Document.Remove(0, target);
                }
            }
        });
    }

    private void UpdateScroll() {
        var target = this._txtEditor!.Document.Lines.Count - 1;
        //var vertOffset = (this._txtEditor.TextArea.TextView.DefaultLineHeight) * target;
        this._txtEditor.ScrollTo(target, 0);
    }
}