#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallServerStreamingFeature(
    MethodInfo mi,
    object?[] mParams,
    string envFile,
    ProjectTypes.ClientConfig cfg,
    IOs io) {
    public async Task<(bool, ResponseServerStreaming)> Run() {
        var (success, resp) = await CallServerStreaming.run(io, mi, mParams, cfg, envFile);
        return (success, resp);
    }
}