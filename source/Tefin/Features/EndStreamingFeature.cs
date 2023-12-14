#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class EndStreamingFeature {
    public void EndClientStreaming(ClientStreamingCallResponse response) {
        ClientStreamingResponse.completeCall(response);
    }

    public DuplexStreamingCallResponse EndDuplexStreaming(DuplexStreamingCallResponse response) {
        return DuplexStreamingResponse.completeCall(response);
    }

    public ServerStreamingCallResponse EndServerStreaming(ServerStreamingCallResponse resp) {
        return ServerStreamingResponse.completeCall(resp);
    }
}