#region

using System.Reflection;

using DynamicData;

using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Grpc.Dynamic;
using Tefin.ViewModels.Types;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.Features;

public class ExportFeature(MethodInfo methodInfo, object?[] methodsParams, List<RequestVariable> variables, object? responseStream = null) {
    public FSharpResult<string, Exception> Export() {
        var displayTypes = SystemType.getTypesForDisplay();
        var actualTypes = SystemType.getTypes().Select(t => t.FullName).ToArray();
         
        var envItems = variables.Select(v => {
            if (v.TypeName == "Google.Protobuf.WellKnownTypes.Timestamp") {
                return EnvConfig.createReqVar(v.Tag, v.JsonPath, "protobufTimestamp");
            }
            
            return EnvConfig.createReqVar(v.Tag, v.JsonPath, displayTypes[actualTypes.IndexOf(v.TypeName)]);
        }).ToArray();
        var sdParam = new SerParam(methodInfo, methodsParams, envItems, responseStream == null ? none<object>() : some(responseStream));
        var exportReqJson = Grpc.Export.requestToJson(sdParam);
        return exportReqJson;
    }
}