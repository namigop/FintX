#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types.TypeEditors;

public abstract class TypeEditorBase<T> : ViewModelBase, ITypeEditor<T> {
    private bool _isEditing;
    private bool _isNull;
    private T? _og;
    private T? _tempValue;

    protected TypeEditorBase(TypeBaseNode node) {
        this.Node = node;
        this._tempValue = node.Value is null ? default : (T)node.Value;
        this._og = this._tempValue;
        this._isNull = this._tempValue is null;
        this.SubscribeTo(x => ((TypeEditorBase<T>)x).IsNull, this.OnIsNullChanged);
    }

    public bool HasChanges {
        get;
        protected set;
    }

    public virtual bool AcceptsNull { get; } = typeof(T).Name.StartsWith("Nullable");

    public virtual string FormattedValue => this.Node.Value?.ToString() ?? "null";

    public bool IsEditing {
        get => this._isEditing;
        set {
            this.RaiseAndSetIfChanged(ref this._isEditing, value);
            if (!this._isEditing && this.HasChanges) {
                this.CommitEdit();
            }
        }
    }

    public bool IsNull {
        get => this._isNull;
        set => this.RaiseAndSetIfChanged(ref this._isNull, value);
    }

    public TypeBaseNode Node { get; }

    public T? TempValue {
        get => this._tempValue;
        set {
            this.HasChanges = true;
            this.RaiseAndSetIfChanged(ref this._tempValue, value);
        }
    }

    public virtual void CommitEdit() {
        try {
            this.Node.Value = this.TempValue;
        }
        catch (ArgumentNullException exc) {
            this.Reset();
            this.Node.Value = this.TempValue;
            this.Io.Log.Error(exc);
            this.IsNull = this.TempValue is null;
        }
        catch (Exception exc) {
            this.Reset();
            this.Io.Log.Error(exc);
        }

        this.HasChanges = false;
    }

    public virtual void Reset() {
        if (this.Node.Value != null) {
            this.TempValue = (T)this.Node.Value;
        }
        else {
            this.TempValue = this._og is null ? default : this._og;
        }

        this.HasChanges = false;
    }

    private void OnIsNullChanged(ViewModelBase obj) {
        var sender = (TypeEditorBase<T>)obj;
        if (sender._isNull) {
            this._og = this._tempValue;
            this.TempValue = default;
        }
        else {
            this.Reset();
        }
    }
}