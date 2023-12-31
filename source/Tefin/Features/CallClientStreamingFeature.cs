#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallClientStreamingFeature {
    private readonly ProjectTypes.ClientConfig _cfg;
    private readonly IOResolver _io;
    private readonly MethodInfo _mi;
    private readonly object?[] _mParams;
    public CallClientStreamingFeature(MethodInfo mi, object?[] mParams, ProjectTypes.ClientConfig cfg, IOResolver io) {
        this._mi = mi;
        this._mParams = mParams;
        this._cfg = cfg;
        this._io = io;
    }

    public async Task<(bool, ResponseClientStreaming)> Run() {
        var (success, resp) = await CallClientStreaming.run(this._io, this._mi, this._mParams, this._cfg);
        return (success, resp);
    }
}