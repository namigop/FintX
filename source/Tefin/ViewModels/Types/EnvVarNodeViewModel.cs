using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Types.TypeEditors;

namespace Tefin.ViewModels.Types;

public class EnvVarNodeViewModel : ViewModelBase {
    private readonly TypeBaseNode _node;
    private string _envVarTag = "";
    private string _ogTag;
    private string _selectedScope;
    private ITypeEditor _defaultValueEditor;
    public const string ProjectScope = "Project";
    public const string ClientScope = "Client";

    public EnvVarNodeViewModel(TypeBaseNode node, string tag = "") {
        this._node = node;
        this.EnvVarTag = tag; //string.IsNullOrWhiteSpace(tag) ? node.Title : tag;
        this._ogTag = tag;
        this.CreateEnvVariableCommand = ReactiveCommand.Create(this.OnCreateEnvVariable);
        this.CancelEnvVariableCommand = ReactiveCommand.Create(this.OnCancelEnvVariable);
        this._selectedScope = this.Scopes[0];
    }

    public string EnvVarTag {
        get => string.IsNullOrWhiteSpace(_envVarTag) ? "" : $"{{{{{this._envVarTag.ToUpperInvariant()}}}}}";
        set {
            this.RaiseAndSetIfChanged(ref this._envVarTag, value.Replace("{", "").Replace("}", ""));
            this.RaisePropertyChanged(nameof(this.IsEnvVarTagVisible));
        }
    }

    public ICommand CreateEnvVariableCommand { get; }
    public ICommand CancelEnvVariableCommand { get; }
   
    public string[] Scopes { get; } = [ProjectScope, ClientScope];

    public string SelectedScope {
        get => this._selectedScope;
        set => this.RaiseAndSetIfChanged(ref _selectedScope, value);
    }

    public void ShowDefault() {
        this.EnvVarTag = this._node.Title;
        var actualType = _node.Type;
        var cur = TypeHelper.getDefault(actualType);
        
        var defaultValueNode = new SystemNode(this.EnvVarTag, actualType, default, cur, null);
        this.DefaultValueEditor = defaultValueNode.Editor;
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
                JsonPath = jsonPath,
                Scope = this.SelectedScope == ProjectScope ? RequestEnvVarScope.Project : RequestEnvVarScope.Client
            };

            methodInfoNode.Variables.Add(v);
            this._ogTag = tag;
            this.IsEnvVarTagCreated = true;
        }
    }

    public bool IsEnvVarTagCreated { get; private set; }

    public bool IsEnvVarTagVisible => !string.IsNullOrWhiteSpace(this.EnvVarTag);

    public ITypeEditor DefaultValueEditor {
        get => this._defaultValueEditor;
        private set => this.RaiseAndSetIfChanged(ref _defaultValueEditor , value);
    }
}