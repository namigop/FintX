#region

using ReactiveUI;

#endregion

namespace Tefin.ViewModels.Types;

public class ExceptionNode : TypeBaseNode {
    private string _message;

    public ExceptionNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(name, type, propInfo, instance, parent) {
        this._message = "";
        if (instance is Exception exc) {
            this.Message = exc.Message;
        }
    }

    public override string FormattedValue {
        get => this.Value == null ? "null" : this.Message;
    }

    public string Message {
        get => this._message;
        set => this.RaiseAndSetIfChanged(ref this._message, value);
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
    }
}