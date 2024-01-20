#region

using System.Reflection;

using Microsoft.FSharp.Core;

using Tefin.Grpc.Dynamic;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.Features;

public class ExportFeature(MethodInfo methodInfo, object?[] methodsParams, object? responseStream = null) {

    public FSharpResult<string, Exception> Export() {
        var sdParam = new SerParam(methodInfo, methodsParams, responseStream == null ? none<object>() : some(responseStream));
        var exportReqJson = Grpc.Export.requestToJson(sdParam);
        return exportReqJson;
    }
}