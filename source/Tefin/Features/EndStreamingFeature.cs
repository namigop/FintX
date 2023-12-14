#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class EndStreamingFeature {
    public async Task<ClientStreamingCallResponse> EndClientStreaming(ClientStreamingCallResponse response) {
        return await ClientStreamingResponse.completeCall(response);
    }

    public async Task<DuplexStreamingCallResponse> EndDuplexStreaming(DuplexStreamingCallResponse response) {
        return await DuplexStreamingResponse.completeCall(response);
    }

    public ServerStreamingCallResponse EndServerStreaming(ServerStreamingCallResponse resp) {
        return ServerStreamingResponse.completeCall(resp);
    }
}