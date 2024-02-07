#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallDuplexStreamingFeature(
    MethodInfo mi,
    object?[] mParams,
    ProjectTypes.ClientConfig cfg,
    IOs io) {
    public async Task<(bool, ResponseDuplexStreaming)> Run() {
        var (success, resp) = await CallDuplexStreaming.run(io, mi, mParams, cfg);
        return (success, resp);
    }
}