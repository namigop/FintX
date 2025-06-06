#region

using Google.Protobuf.WellKnownTypes;
using ReactiveUI;

using Type = System.Type;
#endregion

namespace Tefin.ViewModels.Types;

public class TimestampNode : TypeBaseNode {
    private string _dateTime;
   
    public TimestampNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(
        name, type, propInfo, instance, parent) {
        
        var ts = instance as Timestamp;
        this.EnvVar = new(this);
        if (ts != null) {
            this._dateTime = $"{ts.ToDateTime():O}";
        }
        else {
            this._dateTime = $"{DateTime.Now.ToUniversalTime():O}";
        }
    }
    public EnvVarNodeViewModel EnvVar { get; }
 

    public string DateTimeText {
        get => this._dateTime;
        set => this.RaiseAndSetIfChanged(ref this._dateTime, value);
    }
    
    public override string FormattedTypeName { get; } = $"{{{nameof(Timestamp)}}}";

    public void CommitEdit() {
        if (DateTime.TryParse(this.DateTimeText, out var dt)) {
            this.Value = Timestamp.FromDateTime(dt.ToUniversalTime());
        }
        else {
            this.Reset();
        }
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
    }

    public void Reset() {
        var ts = (Timestamp)this.Value!;
        this.DateTimeText = $"{ts.ToDateTime():O}";
    }
}