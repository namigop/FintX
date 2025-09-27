using Grpc.Core;

namespace Tefin.Features;

public static class ServerHandler {
    public static Task<object> RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        throw new NotImplementedException();
    }
    
    public static Task<object> RunClientStream(string concreteService, string methodName, object requestStream , ServerCallContext context) {
        throw new NotImplementedException();
    }

    public static Task<object> RunServerStream(string concreteService, string methodName, object request, object responseStream, ServerCallContext context) {
        throw new NotImplementedException();
    }

    public static Task<object> RunDuplex(string concreteService, string methodName, object requestStream, object responseStream, ServerCallContext context) {
        throw new NotImplementedException();
    }
}