#region

using System.Reflection;

using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Grpc;
using Tefin.Grpc.Dynamic;

#endregion

namespace Tefin.Features;

public class ImportFeature(IOs io, string file, MethodInfo methodInfo, object? responseStream = null) {
    public (FSharpResult<object[], Exception>, FSharpResult<object, Exception>) Run() {
        var respStream = responseStream == null ? Core.Utils.none<object>() : Core.Utils.some(responseStream);
        var import = Export.importReq(io, new SerParam(methodInfo, [],
            respStream), file);
        return import;
    }
}