#region

using System.Reflection;

using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Grpc;
using Tefin.Grpc.Dynamic;

#endregion

namespace Tefin.Features;

public class ImportFeature {
    private readonly string _file;
    private readonly IOResolver _io;
    private readonly MethodInfo _methodInfo;

    private readonly object? _responseStream;

    public ImportFeature(IOResolver io, string file, MethodInfo methodInfo, object? responseStream = null) {
        this._io = io;
        this._file = file;
        this._methodInfo = methodInfo;

        this._responseStream = responseStream;
    }

    public (FSharpResult<object[], Exception>, FSharpResult<object, Exception>) Run() {
        var respStream = this._responseStream == null ? Core.Utils.none<object>() : Core.Utils.some(this._responseStream);
        var import = Export.importReq(this._io, new SerParam(this._methodInfo, Array.Empty<object>(),
            respStream), this._file);
        return import;
    }
}