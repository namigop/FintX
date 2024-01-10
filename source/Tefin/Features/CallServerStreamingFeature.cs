#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallServerStreamingFeature(MethodInfo mi, object?[] mParams, ProjectTypes.ClientConfig cfg, IOResolver io) {
    public async Task<(bool, ResponseServerStreaming)> Run() {
        var (success, resp) = await CallServerStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}