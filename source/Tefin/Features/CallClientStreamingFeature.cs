#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallClientStreamingFeature(
    MethodInfo mi,
    object?[] mParams,
    ProjectTypes.ClientConfig cfg,
    IOs io) {
    public async Task<(bool, ResponseClientStreaming)> Run() {
        var (success, resp) = await CallClientStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}