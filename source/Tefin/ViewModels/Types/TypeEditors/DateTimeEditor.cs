#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class DateTimeEditor : TypeEditorBase<DateTime> {
    private string _dateTimeText;
    private bool _isUtc;

    public DateTimeEditor(TypeBaseNode node) : base(node) {
        var dateTime = (DateTime)node.Value!;
        this._dateTimeText = $"{dateTime:O}";
        this._isUtc = dateTime.Kind == DateTimeKind.Utc;
    }

    public SystemNode TypeNode => (SystemNode)this.Node;

    public string DateTimeText {
        get => this._dateTimeText;
        set => this.RaiseAndSetIfChanged(ref this._dateTimeText, value);
    }

    public override string FormattedValue => $"{this.TempValue:O}";

    public bool IsUtc {
        get => this._isUtc;
        set {
            this.RaiseAndSetIfChanged(ref this._isUtc, value);
            this.HasChanges = true;
        }
    }

    public override void CommitEdit() {
        if (DateTime.TryParse(this.DateTimeText, out var dt)) {
            if (this.IsUtc) {
                dt = dt.ToUniversalTime();
            }

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
        this.HasChanges = false;
    }
}