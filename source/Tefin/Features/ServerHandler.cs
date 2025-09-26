using Grpc.Core;

namespace Tefin.Features;

public static class ServerHandler {
    public static object RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        throw new NotImplementedException();
    }
}