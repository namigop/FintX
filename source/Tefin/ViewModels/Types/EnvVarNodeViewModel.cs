using System.Windows.Input;

using Google.Protobuf.WellKnownTypes;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Explorer.Client;
using Tefin.ViewModels.Types.TypeEditors;

namespace Tefin.ViewModels.Types;

public class EnvVarNodeViewModel : ViewModelBase {
    private readonly TypeBaseNode _node;
    private string _envVarTag = "";
    private string _ogTag;
    private string _selectedScope;
    private ITypeEditor? _defaultValueEditor;
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

        var envData = VarsStructure.getVarsFromFile(this.Io, Current.EnvFilePath);
        var envVar = envData.Variables.FirstOrDefault(x => x.Name == this.EnvVarTag);
        if (envVar is not null) {
            var res = TypeHelper.tryIndirectCast(envVar.CurrentValue ?? envVar.DefaultValue, actualType);
            if (res.IsOk) {
                this._enVarValue = res.ResultValue;
            }
        }

        if (actualType == typeof(Timestamp)) {
            this._enVarValue ??= Timestamp.FromDateTime(DateTime.UtcNow);
            var tn = new TimestampNode(this.EnvVarTag, actualType, null!, this._enVarValue, null);
            this.DefaultValueEditor = new TimestampEditor(tn);
            return;
        }
        
        var cur = this._enVarValue ?? TypeHelper.getDefault(actualType);
        var defaultValueNode = new SystemNode(this.EnvVarTag, actualType, null!, cur, null);
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
                //remove from the variables. Auto-save will take care of saving the .fxrq file
                methodInfoNode.Variables.Remove(currentVar);
                //removeEnv.Remove(currentVar, methodInfoNode.ClientGroup.Path, Current.Env, this.Io);
            }
        }
    }

    public void CreateEnvVariable(string tag, string jsonPath, string? currentValue = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        
        static NodeContainerVar GetNodeContainerVar(NodeBase node) {
            var parent = node.FindParentNode<NodeBase>(t => t.Parent == null);
            if (parent is MethodInfoNode m)
                return NodeContainerVar.FromMethodInfoNode(m);
            else if (parent is ResponseNode r)
                return NodeContainerVar.FromResponseNode(r);
            else if (parent is ResponseStreamNode rs)
                return NodeContainerVar.FromResponseStreamNode(rs);
            else {
                throw new ArgumentException("Invalid parent node type");
            }

        }
        
        tag = tag.Replace("{", "").Replace("}", "");
        this.EnvVarTag = tag;
        this._ogTag = tag;

        var inst = TypeHelper.indirectCast(currentValue, this._node.Type);
        this._enVarValue = inst;

        var nodeContainerVar = GetNodeContainerVar(this._node); // this._node.FindParentNode<NodeBase>(t => t.Parent == null);  
        var envTag = this.EnvVarTag.ToUpperInvariant();
        var currentVar = nodeContainerVar.Variables.FirstOrDefault(t => t.JsonPath == jsonPath);
        var saveEnvVariable = false;
        if (currentVar != null) {
            if (currentVar.Tag != envTag) {
                //If a new tag is created with the same json path, we have to remove the old one
                nodeContainerVar.Variables.Remove(currentVar);
                var removeEnv = new RemoveEnvVarsFeature();
                removeEnv.Remove(currentVar, nodeContainerVar.ClientPath, Current.Env, this.Io);
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
        nodeContainerVar.Variables.Add(currentVar);
        this.IsEnvVarTagCreated = true;


        var saveEnv = new SaveEnvVarsFeature();
        var strValue = this._enVarValue?.ToString() ?? "";
        if (this._enVarValue is Timestamp timestamp) {
            strValue = timestamp.ToString().Trim('\"');
        }

        if (currentVar.Scope == RequestEnvVarScope.Client) {
            saveEnv.Save(currentVar,
                strValue,
                nodeContainerVar.ClientPath,
                Current.Env,
                this.Io);
        }
        else {
            saveEnv.Save(currentVar,
                strValue,
                nodeContainerVar.ClientPath,
                Current.Env,
                this.Io);
        }
    }

    public bool IsEnvVarTagCreated { get; private set; }

    public bool IsEnvVarTagVisible => !string.IsNullOrWhiteSpace(this.EnvVarTag);

    public ITypeEditor? DefaultValueEditor {
        get => this._defaultValueEditor;
        private set => this.RaiseAndSetIfChanged(ref _defaultValueEditor , value);
    }
}



public class NodeContainerVar(List<RequestVariable> requestVariables, string clientPath) {
    public List<RequestVariable> Variables { get; set; } = requestVariables;
    public string ClientPath { get; set; } = clientPath;

    public static NodeContainerVar FromMethodInfoNode(MethodInfoNode m) {
        return new NodeContainerVar(m.Variables, m.ClientGroup.Path);
    }
    public static NodeContainerVar FromResponseNode(ResponseNode m) {
        return new NodeContainerVar(m.Variables, m.ClientPath);
    }

    public static NodeContainerVar FromResponseStreamNode(ResponseStreamNode rs) {
        return new NodeContainerVar(rs.Variables, rs.ClientPath);
    }
}

