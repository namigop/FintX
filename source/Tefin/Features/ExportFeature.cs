using System.Reflection;

using Microsoft.FSharp.Core;

using Tefin.Grpc.Dynamic;

using static Tefin.Core.Utils;

namespace Tefin.Features;

public class ExportFeature {
    private readonly MethodInfo _methodInfo;
    private readonly object?[] _methodsParams;
    private readonly object? _responseStream;

    public ExportFeature(MethodInfo methodInfo, object?[] methodsParams, object? responseStream = null) {
        this._methodInfo = methodInfo;
        this._methodsParams = methodsParams;
        this._responseStream = responseStream;
    }
    public FSharpResult<string, Exception> Export() {
        var sdParam = new SerParam(this._methodInfo, this._methodsParams, this._responseStream == null ? none<object>() : some(this._responseStream));
        var exportReqJson = Grpc.Export.requestToJson(sdParam);
        return exportReqJson;
    }
}