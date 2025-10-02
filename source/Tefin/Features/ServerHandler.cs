using System.Reflection;

using Grpc.Core;

using Tefin.Core;
using Tefin.Grpc.Sample.Services;

namespace Tefin.Features;

public static class ServerHandler {
    private static Dictionary<string, List<TargetMethod>> _methods = new();
    public static async Task<object> RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        await Task.CompletedTask;
        if (_methods.TryGetValue(concreteService, out var methods)) {
            var tm = methods.FirstOrDefault(m => m.MethodInfo.Name == methodName);
            if (tm != null) {
                var responseType = tm.MethodInfo.ReturnType.GetGenericArguments()[0];
                return Instance.indirectDeserialize(responseType, tm.ScriptText);
            }

            throw new RpcException(new Status(StatusCode.NotFound, $"Method {methodName} not found"));
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"Service not found"));

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