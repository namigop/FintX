#region

using System.Reflection;

using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Grpc.Dynamic;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.Features;

public class ExportFeature(MethodInfo methodInfo, object?[] methodsParams, 
    List<VarDefinition> requestVariables,
    List<VarDefinition> responseVariables,
    List<VarDefinition> requestStreamVariables,
    List<VarDefinition> responseStreamVariables,
    object? responseStream = null) {
    public FSharpResult<string, Exception> Export() {
       static List<RequestEnvVar> Convert(List<VarDefinition> variables) {
            return variables
                .DistinctBy(v => v.JsonPath)
                .Where(v => SystemType.getDisplayType(v.TypeName).Item1)
                .Select(v =>
                    EnvConfig.createReqVar(
                        v.Tag,
                        v.JsonPath,
                        SystemType.getDisplayType(v.TypeName).Item2,
                        v.Scope
                    )).ToList();
        }

        var allVars = AllVariables.Empty();
        allVars.RequestVariables.AddRange(Convert(requestVariables));
        allVars.ResponseVariables.AddRange(Convert(responseVariables));
        allVars.RequestStreamVariables.AddRange(Convert(requestStreamVariables));
        allVars.ResponseStreamVariables.AddRange(Convert(responseStreamVariables));
        
        var sdParam = new SerParam(methodInfo, methodsParams, allVars, responseStream == null ? none<object>() : some(responseStream));
        var exportReqJson = Grpc.Export.requestToJson(sdParam);
        return exportReqJson;
    }
}