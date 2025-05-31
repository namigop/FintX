#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CallUnaryFeature(MethodInfo mi, object?[] mParams, string envFile, ProjectTypes.ClientConfig cfg, IOs io) {
    public async Task<(bool, ResponseUnary)> Run() {
        var (success, resp) = await CallUnary.run(io, mi, mParams, cfg, envFile);
        return (success, resp);
    }
}