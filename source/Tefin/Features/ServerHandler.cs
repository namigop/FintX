using System.Reflection;
using Grpc.Core;
using Tefin.Core;
using Tefin.Core.Scripting;

namespace Tefin.Features;

public static class ServerHandler {
    
    private static Dictionary<string, ScriptEnv> _env = new();
    public static async Task<object> RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        await Task.CompletedTask;
        if (_env.TryGetValue(concreteService, out var env)) {
            var (json, responseType) = await env.RunUnary(methodName, request, context);
            return Instance.indirectDeserialize(responseType, json);
        }

        throw new RpcException(new Status(StatusCode.NotFound, $"Service not found"));
    }
    
    public static Task<object> RunClientStream(string concreteService, string methodName, object requestStream , ServerCallContext context) {
        throw new NotImplementedException("Mocks are not supported for client streaming. Try out FintX Enterprise instead at https://fintx.dev");
    }

    public static Task RunServerStream(string concreteService, string methodName, object request, object responseStream, ServerCallContext context) {
        throw new NotImplementedException("Mocks are not supported for server streaming. Try out FintX Enterprise instead at https://fintx.dev");
    }

    public static Task RunDuplex(string concreteService, string methodName, object requestStream, object responseStream, ServerCallContext context) {
        throw new NotImplementedException("Mocks are not supported for duplex streaming. Try out FintX Enterprise instead at https://fintx.dev");
    }

    public static void Register(string serviceName, MethodInfo methodInfo, Func<string> getScriptText) {
        if (!_env.TryGetValue(serviceName, out var env)) {
            env = new ScriptEnv(serviceName, Script.createEngine(serviceName), []);
            _env[serviceName] = env;
        }
 
        env.TryAddMethod(methodInfo, getScriptText);;
    }

    public static void UnRegister(string serviceName, MethodInfo methodInfo) {
        if (!_env.TryGetValue(serviceName, out var env)) {
            env = new ScriptEnv(serviceName, Script.createEngine(serviceName), []);
            _env[serviceName] = env;
        }

        env?.TryRemoveMethod(methodInfo);
    }

    public record TargetMethod(MethodInfo MethodInfo, Func<string> GetScriptText);

    public record ScriptEnv(string ServiceName, ScriptEngine Engine, List<TargetMethod> Methods) {
        public async Task<(string, Type)> RunUnary(string method, object request, ServerCallContext context) {
            var tm = Methods.FirstOrDefault(m => m.MethodInfo.Name == method);
            if (tm != null) {
                var responseType = tm.MethodInfo.ReturnType.GetGenericArguments()[0];
                var gl = new ServerGlobals(request, null, null, context, Resolver.value);
                var script = tm.GetScriptText();
                var res = await ScriptExec.start(Resolver.value, this.Engine, script, gl, $"{ServiceName}-{method}");
                if (res.IsOk) {
                    return (res.ResultValue, responseType);
                }

                return (res.ErrorValue.ToString(), responseType);

            }

            throw new RpcException(new Status(StatusCode.NotFound, $"Method {method} not found"));
        }

        public void TryAddMethod(MethodInfo methodInfo,  Func<string> getScriptText) {
            var tm = Methods.FirstOrDefault(m => m.MethodInfo.Name == methodInfo.Name);
            if (tm == null) {
                Methods.Add(new TargetMethod(methodInfo, getScriptText));
            }
        }

        public void TryRemoveMethod(MethodInfo methodInfo) {
            var tm = Methods.FirstOrDefault(m => m.MethodInfo.Name == methodInfo.Name);
            if (tm != null) {
                Methods.Remove(tm);
            }
        }
    }
    
}