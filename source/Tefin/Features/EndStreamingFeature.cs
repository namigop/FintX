#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class EndStreamingFeature {
    public async Task<ClientStreamingCallResponse> EndClientStreaming(ClientStreamingCallResponse response) =>
        await ClientStreamingResponse.completeCall(response);

    public async Task<DuplexStreamingCallResponse> EndDuplexStreaming(DuplexStreamingCallResponse response) =>
        await DuplexStreamingResponse.completeCall(response);

    public async Task<ServerStreamingCallResponse> EndServerStreaming(ServerStreamingCallResponse resp) =>
        await ServerStreamingResponse.completeCall(resp);
}