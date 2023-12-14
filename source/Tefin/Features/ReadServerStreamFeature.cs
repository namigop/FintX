#region

using System.Runtime.CompilerServices;
using System.Threading;

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class ReadServerStreamFeature {
    public async IAsyncEnumerable<object> ReadResponseStream(ServerStreamingCallResponse resp, [EnumeratorCancellation] CancellationToken token) {
        while (!token.IsCancellationRequested && await resp.CallInfo.MoveNext(resp.CallResult, token)) {
            var i = resp.CallInfo.GetCurrent(resp.CallResult);
            yield return i;
        }
    }
    public async Task<ServerStreamingCallResponse> CompleteRead(ServerStreamingCallResponse resp) {
        resp = await ServerStreamingResponse.getResponseHeader(resp);
        return resp;
    }
}