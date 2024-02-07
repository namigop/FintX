#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class WriteClientStreamFeature {
    public async Task<ClientStreamingCallResponse> CompleteWrite(ClientStreamingCallResponse response) {
        response = await ClientStreamingResponse.completeWrite(response);
        response = await ClientStreamingResponse.getResponseHeader(response);
        return response;
        //return ClientStreamingResponse.completeCall(response);
    }

    public async Task Write(ClientStreamingCallResponse response, object instance) =>
        await ClientStreamingResponse.write(response, instance);
}