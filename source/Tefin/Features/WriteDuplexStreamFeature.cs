#region

using Tefin.Grpc.Execution;

#endregion

namespace Tefin.Features;

public class WriteDuplexStreamFeature {
    public async Task<DuplexStreamingCallResponse> CompleteWrite(DuplexStreamingCallResponse response) {
        response = await DuplexStreamingResponse.completeWrite(response);
        response = await DuplexStreamingResponse.getResponseHeader(response);
        return response;
        //return DuplexStreamingResponse.completeCall(response);
    }

    public async Task Write(DuplexStreamingCallResponse response, object? instance) {
        await DuplexStreamingResponse.write(response, instance);
    }
}