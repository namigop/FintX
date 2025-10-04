using Grpc.Core;

using Tefin.Core;

namespace Tefin.Features;

public class ServerGlobals(object? request, object? requestStream, object? responseStream, ServerCallContext context, IOs io) {
    public ServerCallContext Context => context;

    public Log.ILog Log => io.Log;

    public Task Sleep(int ms) => Task.Delay(ms);
    public async  IAsyncEnumerable<string> ReadStream() {
        if (requestStream == null)
            yield break;
        
        var mi = requestStream.GetType().GetMethod("MoveNext")!;
        var pi = requestStream.GetType().GetProperty("Current")!;
        var ok = mi.Invoke(requestStream, []) as Task<bool>;
        
        while (await (mi.Invoke(requestStream, []) as Task<bool>)) {
            var value = Instance.jsonSerialize(pi.GetValue(requestStream)); 
            yield return value;
        }
    }
}