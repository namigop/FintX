using System.Windows.Input;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.ViewModels.Types.TypeEditors;

namespace Tefin.ViewModels.Types;

public class EnvVarNodeViewModel : ViewModelBase {
    private readonly TypeBaseNode _node;
    private string _envVarTag = "";
    private string _ogTag;
    private string _selectedScope;
    private ITypeEditor _defaultValueEditor;
    private object? _enVarValue;
    public const string ProjectScope = "Project";
    public const string ClientScope = "Client";

    public EnvVarNodeViewModel(TypeBaseNode node, string tag = "") {
        this._node = node;
        this.EnvVarTag = tag; //string.IsNullOrWhiteSpace(tag) ? node.Title : tag;
        this._ogTag = tag;
        this.CreateEnvVariableCommand = ReactiveCommand.Create(this.OnCreateEnvVariable);
        this.CancelEnvVariableCommand = ReactiveCommand.Create(this.OnCancelEnvVariable);
        this.RemoveEnvVariableCommand = ReactiveCommand.Create(this.OnRemoveEnvVariable);
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
    public ICommand RemoveEnvVariableCommand { get; }

    public string[] Scopes { get; } = [ProjectScope, ClientScope];

    public string SelectedScope {
        get => this._selectedScope;
        set => this.RaiseAndSetIfChanged(ref _selectedScope, value);
    }

    public void ShowDefault() {
        this.EnvVarTag = this._node.Title;
        var actualType = _node.Type;
        var cur = this._enVarValue ?? TypeHelper.getDefault(actualType);
        
        var defaultValueNode = new SystemNode(this.EnvVarTag, actualType, default, cur, null);
        this.DefaultValueEditor = defaultValueNode.Editor;
    }

    public void Reset() {
        this.EnvVarTag = this._ogTag;
    }

    private void OnCreateEnvVariable() {
        var tag = this.EnvVarTag.ToUpperInvariant();
        var jsonPath = this._node.GetJsonPath();
        //var cur = TypeHelper.getDefault(this._node.Type);
        if (this.DefaultValueEditor is not null) {
            this.DefaultValueEditor.Node.IsEditing = false;
        }
       
        this.CreateEnvVariable(tag, jsonPath, this.DefaultValueEditor?.FormattedValue);
        
    }

    private void OnCancelEnvVariable() {
        this.EnvVarTag = this._ogTag;
    } 
    
    private void OnRemoveEnvVariable() {
        this.EnvVarTag = "";
        this._ogTag = "";
        var methodInfoNode = this._node.FindParentNode<MethodInfoNode>();
        if (methodInfoNode != null) {
            var jsonPath = this._node.GetJsonPath();
            var removeEnv = new RemoveEnvVarsFeature();
            var currentVar = methodInfoNode.Variables.FirstOrDefault(t => t.JsonPath == jsonPath);
            if (currentVar != null) {
                methodInfoNode.Variables.Remove(currentVar);
                removeEnv.Remove(currentVar, methodInfoNode.ClientGroup.Path, Current.Env, this.Io);
            }
        }
    }

    public void CreateEnvVariable(string tag, string jsonPath, string? currentValue = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        
        tag = tag.Replace("{", "").Replace("}", "");
        this.EnvVarTag = tag;
        this._ogTag = tag;
        var inst = TypeHelper.indirectCast(currentValue, this._node.Type);
        this._enVarValue = inst;
      
        var removeEnv = new RemoveEnvVarsFeature();
        var methodInfoNode = this._node.FindParentNode<MethodInfoNode>();
        var envTag = this.EnvVarTag.ToUpperInvariant();
        if (methodInfoNode != null) {
            var currentVar = methodInfoNode.Variables.FirstOrDefault(t => t.JsonPath == jsonPath);
            var saveEnvVariable = false;
            if (currentVar != null) {
                if (currentVar.Tag != envTag) {
                    //If a new tag is created with the same json path, we have to remove the old one
                    methodInfoNode.Variables.Remove(currentVar);
                    removeEnv.Remove(currentVar, methodInfoNode.ClientGroup.Path, Current.Env, this.Io);
                    saveEnvVariable = true;
                }
                else {
                    saveEnvVariable = false;
                }
            }
            else {
                //New variable was created. save it
                saveEnvVariable = true;
            }

            if (!saveEnvVariable)
                return;
            
            currentVar = new RequestVariable() {
                Tag = envTag,
                TypeName = this._node.Type.FullName!,
                JsonPath = jsonPath,
                Scope = this.SelectedScope == ProjectScope ? RequestEnvVarScope.Project : RequestEnvVarScope.Client
            };
            methodInfoNode.Variables.Add(currentVar);
            this.IsEnvVarTagCreated = true;

            
            var saveEnv = new SaveEnvVarsFeature();
            if (currentVar.Scope == RequestEnvVarScope.Client) {
                saveEnv.Save(currentVar,
                    this._enVarValue?.ToString() ?? "",
                    methodInfoNode.ClientGroup.Path,
                    Current.Env,
                    this.Io);
            }
            else {
                saveEnv.Save(currentVar,
                    this._enVarValue?.ToString() ?? "",
                    methodInfoNode.ClientGroup.Path,
                    Current.Env,
                    this.Io);
            }
        }
    }

    public bool IsEnvVarTagCreated { get; private set; }

    public bool IsEnvVarTagVisible => !string.IsNullOrWhiteSpace(this.EnvVarTag);

    public ITypeEditor? DefaultValueEditor {
        get => this._defaultValueEditor;
        private set => this.RaiseAndSetIfChanged(ref _defaultValueEditor , value);
    }
}