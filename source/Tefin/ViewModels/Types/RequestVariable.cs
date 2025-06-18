using System.Security.Cryptography;

using Tefin.Core;

namespace Tefin.ViewModels.Types;

public class RequestVariable {
    public string Tag { get; init; } = "";
    public string TypeName { get; init; }= "";
    public string JsonPath { get; init; }= "";
    public RequestEnvVarScope Scope { get; init; }
    
}

public class AllVariableDefinitions {
    public List<RequestVariable> RequestVariables { get; init; } = new();
    public List<RequestVariable> ResponseVariables { get; init; } = new();
    public List<RequestVariable> RequestStreamVariables { get; init; } = new();
    public List<RequestVariable> ResponseStreamVariables { get; init; } = new();

    public static AllVariableDefinitions From(AllVariables allVars) {
        var allVarDefs = new AllVariableDefinitions();
        allVarDefs.RequestVariables.AddRange(
            allVars.RequestVariables.Select(v => new RequestVariable {
                Tag = v.Tag, TypeName = v.Tag, JsonPath = v.JsonPath, Scope = v.Scope
            }));

        allVarDefs.ResponseVariables.AddRange(
            allVars.ResponseVariables.Select(v => new RequestVariable {
                Tag = v.Tag, TypeName = v.Tag, JsonPath = v.JsonPath, Scope = v.Scope
            }));
        
        allVarDefs.RequestStreamVariables.AddRange(
            allVars.RequestStreamVariables.Select(v => new RequestVariable {
                Tag = v.Tag, TypeName = v.Tag, JsonPath = v.JsonPath, Scope = v.Scope
            }));
        
        allVarDefs.ResponseStreamVariables.AddRange(
            allVars.ResponseStreamVariables.Select(v => new RequestVariable {
                Tag = v.Tag, TypeName = v.Tag, JsonPath = v.JsonPath, Scope = v.Scope
            }));
        
        return allVarDefs;
    }
    
    public static AllVariables To(AllVariableDefinitions allVars) {
        var allVarDefs = new AllVariables([], [], [], []);
        allVarDefs.RequestVariables.AddRange(allVars.RequestVariables.Select(v => new RequestEnvVar(v.Tag, v.JsonPath, v.TypeName, v.Scope)));
        allVarDefs.ResponseVariables.AddRange(allVars.ResponseVariables.Select(v => new RequestEnvVar(v.Tag, v.JsonPath, v.TypeName, v.Scope)));
        allVarDefs.RequestStreamVariables.AddRange(allVars.RequestStreamVariables.Select(v => new RequestEnvVar(v.Tag, v.JsonPath, v.TypeName, v.Scope)));
        allVarDefs.ResponseStreamVariables.AddRange(allVars.ResponseStreamVariables.Select(v => new RequestEnvVar(v.Tag, v.JsonPath, v.TypeName, v.Scope)));
        return allVarDefs;
    }
}

 