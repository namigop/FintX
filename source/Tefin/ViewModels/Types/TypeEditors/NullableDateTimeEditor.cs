#region

using ReactiveUI;

using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public class NullableDateTimeEditor : TypeEditorBase<DateTime?> {
    private string _dateTimeText;
    private bool _isUtc;

    public NullableDateTimeEditor(TypeBaseNode node) : base(node) {
        var dateTime = (DateTime)(node.Value ?? DateTime.Now.AddDays(1));
        this._dateTimeText = $"{dateTime:O}";
        this._isUtc = dateTime.Kind == DateTimeKind.Utc;
    }

    public string DateTimeText {
        get => this._dateTimeText;
        set {
            this.RaiseAndSetIfChanged(ref this._dateTimeText, value);
            this.HasChanges = true;
        }
    }

    public override string FormattedValue => this.TempValue == null ? "null" : $"{this.TempValue:O}";

    public bool IsUtc {
        get => this._isUtc;
        set {
            this.RaiseAndSetIfChanged(ref this._isUtc, value);
            this.HasChanges = true;
        }
    }

    public SystemNode TypeNode => (SystemNode)this.Node;

    public override void CommitEdit() {
        if (this.IsNull) {
            base.CommitEdit();
            return;
        }

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
        if (this.TempValue != null) {
            this.DateTimeText = $"{this.TempValue:O}";
        }
        else {
            this.TempValue = (this.IsUtc ? DateTime.UtcNow : DateTime.Now).Then(d => d.AddDays(1));
            this.DateTimeText = $"{this.TempValue:O}";
        }
    }
}