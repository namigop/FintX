using System.Reflection;

using Tefin.Core;
using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class CallDuplexStreamingFeature(MethodInfo mi, object[] mParams, CallConfig cfg, IOResolver io) {

    public async Task<(bool, ResponseDuplexStreaming)> Run() {
        var (success, resp) = await CallDuplexStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}