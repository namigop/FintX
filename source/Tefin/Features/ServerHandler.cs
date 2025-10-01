using System.Reflection;

using Grpc.Core;

namespace Tefin.Features;

public static class ServerHandler {
    private static Dictionary<string, List<TargetMethod>> _methods = new();
    public static Task<object> RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        throw new NotImplementedException();
    }
    
    public static Task<object> RunClientStream(string concreteService, string methodName, object requestStream , ServerCallContext context) {
        throw new NotImplementedException();
    }

    public static Task RunServerStream(string concreteService, string methodName, object request, object responseStream, ServerCallContext context) {
        throw new NotImplementedException();
    }

    public static Task RunDuplex(string concreteService, string methodName, object requestStream, object responseStream, ServerCallContext context) {
        throw new NotImplementedException();
    }

    public static void Register(string serviceName, MethodInfo methodInfo, string scriptText) {
        var tm = new TargetMethod(methodInfo, scriptText);
        if (!_methods.ContainsKey(serviceName)) {
            _methods[serviceName] = [];
        }
        
        if (!_methods[serviceName].Contains(tm))
            _methods[serviceName].Add(tm);
    }
    
    public static void UnRegister(string serviceName, MethodInfo methodInfo, string scriptText) {
        var tm = new TargetMethod(methodInfo, scriptText);
        if (!_methods.ContainsKey(serviceName)) {
            _methods[serviceName] = [];
        }
        
        if (_methods[serviceName].Contains(tm))
            _methods[serviceName].Remove(tm);
    }

    public record TargetMethod(MethodInfo MethodInfo, string ScriptText);
}