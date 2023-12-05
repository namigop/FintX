using System.Runtime.CompilerServices;
using System.Threading;

using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class ReadDuplexStreamFeature {

    public async IAsyncEnumerable<object> ReadResponseStream(DuplexStreamingCallResponse resp, [EnumeratorCancellation] CancellationToken token) {
        while (await resp.CallInfo.MoveNext(token)) {
            if (token.IsCancellationRequested)
                break;
            var i = resp.CallInfo.GetCurrent();
            yield return i;
        }
    }
}