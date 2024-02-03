#region

using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

#endregion

namespace Tefin.Utils;

public class CharFoldingStrategy(char openingChar, char closingChar) {
    /// <summary>
    ///     Creates a new BraceFoldingStrategy.
    /// </summary>
    public CharFoldingStrategy() : this('{', '}') {
    }

    /// <summary>
    ///     Gets/Sets the closing brace. The default value is '}'.
    /// </summary>
    private char ClosingBrace { get; } = closingChar;

    /// <summary>
    ///     Gets/Sets the opening brace. The default value is '{'.
    /// </summary>
    private char OpeningBrace { get; } = openingChar;

    public void UpdateFoldings(FoldingManager manager, TextDocument document) {
        var newFoldings = this.CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    /// <summary>
    ///     Create <see cref="NewFolding" />s for the specified document.
    /// </summary>
    private IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset) {
        firstErrorOffset = -1;
        return this.CreateNewFoldings(document);
    }

    /// <summary>
    ///     Create <see cref="NewFolding" />s for the specified document.
    /// </summary>
    private IEnumerable<NewFolding> CreateNewFoldings(ITextSource document) {
        var newFoldings = new List<NewFolding>();

        var startOffsets = new Stack<int>();
        var lastNewLineOffset = 0;
        var openingBrace = this.OpeningBrace;
        var closingBrace = this.ClosingBrace;
        for (var i = 0; i < document.TextLength; i++) {
            var c = document.GetCharAt(i);
            if (c == openingBrace) {
                startOffsets.Push(i);
            }
            else if (c == closingBrace && startOffsets.Count > 0) {
                var startOffset = startOffsets.Pop();
                // don't fold if opening and closing brace are on the same line
                if (startOffset < lastNewLineOffset) {
                    newFoldings.Add(new NewFolding(startOffset, i + 1));
                }
            }
            else if (c == '\n' || c == '\r') {
                lastNewLineOffset = i + 1;
            }
        }

        newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return newFoldings;
    }
}