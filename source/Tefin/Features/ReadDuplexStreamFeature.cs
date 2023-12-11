using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

using Tefin.Grpc.Execution;

namespace Tefin.Features;

public class ReadDuplexStreamFeature {

    public async IAsyncEnumerable<object> ReadResponseStream(DuplexStreamingCallResponse resp, [EnumeratorCancellation] CancellationToken token) {
        while (!token.IsCancellationRequested && await resp.CallInfo.MoveNext(resp.ResponseStream, token)) {
            var i = resp.CallInfo.GetCurrent(resp.ResponseStream);
            yield return i;
        }
    }
}