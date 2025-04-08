using System.Windows.Input;

using ReactiveUI;

namespace Tefin.ViewModels.Types;

public class EnvVarNodeViewModel : ViewModelBase {
    private readonly TypeBaseNode _node;
    private string _envVarTag = "";
    private string _ogTag;

    public EnvVarNodeViewModel(TypeBaseNode node, string tag = "") {
        this._node = node;
        this.EnvVarTag = tag; //string.IsNullOrWhiteSpace(tag) ? node.Title : tag;
        this._ogTag = tag;
        this.CreateEnvVariableCommand = ReactiveCommand.Create(this.OnCreateEnvVariable);
        this.CancelEnvVariableCommand = ReactiveCommand.Create(this.OnCancelEnvVariable);
    }
    public string EnvVarTag {
        get => string.IsNullOrWhiteSpace(_envVarTag) ? "" : $"{{{{{this._envVarTag.ToUpperInvariant()}}}}}";
        set {
            this.RaiseAndSetIfChanged(ref this._envVarTag, value.Replace("{{", "").Replace("}}", ""));
            this.RaisePropertyChanged(nameof(this.IsEnvVarTagVisible));
        }
    }
    public ICommand CreateEnvVariableCommand { get; }
    public ICommand CancelEnvVariableCommand { get; }

    public void ShowDefault() {
        this.EnvVarTag = this._node.Title;
    }
    
    public void Reset() {
        this.EnvVarTag = this._ogTag;
    }
    private void OnCreateEnvVariable() {
        var tag = this.EnvVarTag.ToUpperInvariant();
        var jsonPath = this._node.GetJsonPath();
        this.CreateEnvVariable(tag, jsonPath);
    }
    
    private void OnCancelEnvVariable() {
        this.EnvVarTag = this._ogTag;
    }

    public void CreateEnvVariable(string tag, string jsonPath) {
        tag = tag.Replace("{{", "").Replace("}}", "");
        this.EnvVarTag = tag;
        var methodInfoNode = this._node.FindParentNode<MethodInfoNode>();
        if (methodInfoNode != null && !methodInfoNode.Variables.Exists(t => t.JsonPath == jsonPath)) {
            var v = new RequestVariable() {
                Tag = $"{{{{{this.EnvVarTag.ToUpperInvariant()}}}}}",
                TypeName = this._node.Type.FullName!,
                JsonPath = jsonPath
            };
            
            methodInfoNode.Variables.Add(v);
            this._ogTag = tag;
        }
    }

    public bool IsEnvVarTagVisible => !string.IsNullOrWhiteSpace(this.EnvVarTag);

}