#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class DateTimeOffsetEditor : TypeEditorBase<DateTimeOffset> {
    private string _dateTimeText;

    public DateTimeOffsetEditor(TypeBaseNode node) : base(node) {
        var dateTime = (DateTimeOffset)node.Value!;
        this._dateTimeText = $"{dateTime:O}";
    }

    public string DateTimeText {
        get => this._dateTimeText;
        set {
            this.RaiseAndSetIfChanged(ref this._dateTimeText, value);
            this.HasChanges = true;
        }
    }

    public override string FormattedValue {
        get => $"{this.TempValue:O}";
    }

    public override void CommitEdit() {
        if (DateTimeOffset.TryParse(this.DateTimeText, out var dt)) {
            this.TempValue = dt;
            this.Node.Value = dt;
            this.DateTimeText = $"{dt:O}";
        }
        else {
            this.Reset();
        }
    }

    public override void Reset() {
        this.DateTimeText = $"{this.TempValue:O}";
    }
}