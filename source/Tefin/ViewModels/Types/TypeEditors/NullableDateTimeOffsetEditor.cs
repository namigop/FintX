namespace Tefin.ViewModels.Types.TypeEditors;

using ReactiveUI;

public class NullableDateTimeOffsetEditor : TypeEditorBase<DateTimeOffset?> {
    private string _dateTimeText;

    public NullableDateTimeOffsetEditor(TypeBaseNode node) : base(node) {
        var dateTime = (DateTimeOffset)(node.Value ?? DateTimeOffset.Now.AddDays(1));
        this._dateTimeText = $"{dateTime:O}";
    }

    public string DateTimeText {
        get => this._dateTimeText;
        set {
            this.RaiseAndSetIfChanged(ref this._dateTimeText, value);
            this.HasChanges = true;
        }
    }

    public override string FormattedValue => this.TempValue == null ? "null" : $"{this.TempValue:O}";

    public override void CommitEdit() {
        if (this.IsNull) {
            base.CommitEdit();
            return;
        }

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