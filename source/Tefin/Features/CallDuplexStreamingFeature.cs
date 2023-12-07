using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class CallDuplexStreamingFeature {
    private readonly MethodInfo _mi;
    private readonly object[] _mParams;
    private readonly ProjectTypes.ClientConfig _cfg;
    private readonly IOResolver _io;
    public CallDuplexStreamingFeature(MethodInfo mi, object[] mParams, ProjectTypes.ClientConfig cfg, IOResolver io) {
        this._mi = mi;
        this._mParams = mParams;
        this._cfg = cfg;
        this._io = io;
    }

    public async Task<(bool, ResponseDuplexStreaming)> Run() {
        var (success, resp) = await CallDuplexStreaming.run(this._io, this._mi, this._mParams, this._cfg);
        return (success, resp);
    }
}