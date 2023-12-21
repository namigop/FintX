#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class CompositeResponseFeature {
    public async Task<(Type, object)> BuildClientStreamResponse(ClientStreamingCallResponse response) {
        return await ClientStreamingResponse.emitClientStreamResponse(response);
    }
    public async Task<(Type, object)> BuildUnaryAsyncResponse(ClientStreamingCallResponse response) {
        //TODO
        return await ClientStreamingResponse.emitClientStreamResponse(response);
    }
}