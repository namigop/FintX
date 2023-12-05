using System.Runtime.CompilerServices;
using System.Threading;

using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class ReadServerStreamFeature {

    public async Task<ServerStreamingCallResponse> CompleteRead(ServerStreamingCallResponse resp) {
        resp = await ServerStreamingResponse.getResponseHeader(resp);
        return ServerStreamingResponse.completeCall(resp);
    }

    public async IAsyncEnumerable<object> ReadResponseStream(ServerStreamingCallResponse resp, [EnumeratorCancellation] CancellationToken token) {
        while (await resp.CallInfo.MoveNext(token)) {
            var i = resp.CallInfo.GetCurrent();
            yield return i;
        }
    }
}