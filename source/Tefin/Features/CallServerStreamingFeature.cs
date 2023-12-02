using System.Reflection;

using Tefin.Core;
using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class CallServerStreamingFeature(MethodInfo mi, object[] mParams, CallConfig cfg, IOResolver io) {

    public async Task<(bool, ResponseServerStreaming)> Run() {
        var (success, resp) = await CallServerStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}