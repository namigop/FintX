#region

using System.Windows.Input;

using Google.Protobuf.WellKnownTypes;

using ReactiveUI;

using Type = System.Type;

#endregion

namespace Tefin.ViewModels.Types;

public class TimestampNode : TypeBaseNode {
    private string _dateTime;
    private string _envVarTag;

    public TimestampNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode? parent) : base(
        name, type, propInfo, instance, parent) {
        var ts = instance as Timestamp;
        if (ts != null) {
            this._dateTime = $"{ts.ToDateTime():O}";
        }
        else {
            this._dateTime = $"{DateTime.Now.ToUniversalTime():O}";
        }
        
        this.CreateEnvVariableCommand = ReactiveCommand.Create(this.OnCreateEnvVariable);

    }
    public bool IsEnvVarTagVisible => !string.IsNullOrWhiteSpace(this.EnvVarTag);
    public string EnvVarTag {
        get => this._envVarTag;
        set {
            this.RaiseAndSetIfChanged(ref this._envVarTag, value);
            this.RaisePropertyChanged(nameof(this.IsEnvVarTagVisible));
        }
    }

    public string DateTimeText {
        get => this._dateTime;
        set => this.RaiseAndSetIfChanged(ref this._dateTime, value);
    }
    public ICommand CreateEnvVariableCommand { get; }

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
    
    private void OnCreateEnvVariable() {
        var tag = $"{{{{{this.Title.ToUpperInvariant()}}}}}";
        var jsonPath = GetJsonPath();
        this.CreateEnvVariable(tag, jsonPath);
    }
    public void CreateEnvVariable(string tag, string jsonPath) {
        this.EnvVarTag = tag;
        var methodInfoNode = this.FindParentNode<MethodInfoNode>();
        if (methodInfoNode != null && !methodInfoNode.Variables.Exists(t => t.JsonPath == jsonPath)) {
            var v = new RequestVariable() {
                Tag = tag,
                TypeName = this.Type.FullName!,
                JsonPath = jsonPath
            };
            
            methodInfoNode.Variables.Add(v);
        }
    }
}