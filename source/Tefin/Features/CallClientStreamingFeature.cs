using System.Reflection;

using Tefin.Core;
using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class CallClientStreamingFeature(MethodInfo mi, object[] mParams, CallConfig cfg, IOResolver io) {

    public async Task<(bool, ResponseClientStreaming)> Run() {
        var (success, resp) = await CallClientStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}