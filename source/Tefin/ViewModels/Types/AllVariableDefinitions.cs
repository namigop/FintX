using Tefin.Core;
using Tefin.Core.Reflection;

namespace Tefin.ViewModels.Types;

public class AllVariableDefinitions {
    public List<VarDefinition> RequestVariables { get; init; } = new();
    public List<VarDefinition> ResponseVariables { get; init; } = new();
    public List<VarDefinition> RequestStreamVariables { get; init; } = new();
    public List<VarDefinition> ResponseStreamVariables { get; init; } = new();

    public static AllVariableDefinitions From(AllVariables allVars) {
        var allVarDefs = new AllVariableDefinitions();
        allVarDefs.RequestVariables.AddRange(
            allVars.RequestVariables.Select(v => new VarDefinition {
                Tag = v.Tag, TypeName = SystemType.getActualType(v.Type), JsonPath = v.JsonPath, Scope = v.Scope
            }));

        allVarDefs.ResponseVariables.AddRange(
            allVars.ResponseVariables.Select(v => new VarDefinition {
                Tag = v.Tag, TypeName = SystemType.getActualType(v.Type), JsonPath = v.JsonPath, Scope = v.Scope
            }));
        
        allVarDefs.RequestStreamVariables.AddRange(
            allVars.RequestStreamVariables.Select(v => new VarDefinition {
                Tag = v.Tag, TypeName = SystemType.getActualType(v.Type), JsonPath = v.JsonPath, Scope = v.Scope
            }));
        
        allVarDefs.ResponseStreamVariables.AddRange(
            allVars.ResponseStreamVariables.Select(v => new VarDefinition {
                Tag = v.Tag, TypeName = SystemType.getActualType(v.Type), JsonPath = v.JsonPath, Scope = v.Scope
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