using System.Reflection;

using Grpc.Core;

using Tefin.Core;
using Tefin.Core.Scripting;
using Tefin.Grpc.Sample.Services;

namespace Tefin.Features;

public class ServerGlobals(object? request, object? requestStream, object? responseStream, ServerCallContext context, IOs io) {
    public ServerCallContext Context => context;

    public Log.ILog Log => io.Log;

    public Task Sleep(int ms) => Task.Delay(ms);
    public async  IAsyncEnumerable<string> ReadStream() {
        var mi = requestStream.GetType().GetMethod("MoveNext");
        var pi = requestStream.GetType().GetProperty("Current");
        var ok = mi.Invoke(requestStream, []) as Task<bool>;
        
        while (await (mi.Invoke(requestStream, []) as Task<bool>)) {
            var value = Instance.jsonSerialize(pi.GetValue(requestStream)); 
            yield return value;
        }
    }
}
public static class ServerHandler {
    
    private static Dictionary<string, List<ScriptEnv>> _env = new();
    public static async Task<object> RunUnary(string concreteService, string methodName, object request, ServerCallContext context) {
        await Task.CompletedTask;
        if (_env.TryGetValue(concreteService, out var envs)) {
            var tm = envs.FirstOrDefault(m => m.ServiceName == concreteService);
            if (tm != null) {
                var (json, responseType) = await tm.RunUnary(methodName, request, context);
                return Instance.indirectDeserialize(responseType, json);
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
        if (!_env.TryGetValue(serviceName, out var value)) {
            value = ([]);
            _env[serviceName] = value;
        }

        var env = value.FirstOrDefault(t => t.ServiceName == serviceName);
        if (env == null) {
            env = new ScriptEnv(serviceName, Script.createEngine(serviceName), new List<TargetMethod>());
            value.Add(env);
        }
        
        env.TryAddMethod(methodInfo, scriptText);;
    }

    public static void UnRegister(string serviceName, MethodInfo methodInfo) {
        if (!_env.TryGetValue(serviceName, out var value)) {
            value = ([]);
            _env[serviceName] = value;
        }

        var env = value.FirstOrDefault(t => t.ServiceName == serviceName);
        env?.TryRemoveMethod(methodInfo);

    }

    public record TargetMethod(MethodInfo MethodInfo, string ScriptText);

    public record ScriptEnv(string ServiceName, ScriptEngine Engine, List<TargetMethod> Methods) {
        public async Task<(string, Type)> RunUnary(string method, object request, ServerCallContext context) {
            var tm = Methods.FirstOrDefault(m => m.MethodInfo.Name == method);
            if (tm != null) {
                var responseType = tm.MethodInfo.ReturnType.GetGenericArguments()[0];
                var gl = new ServerGlobals(request, null, null, context, Resolver.value);
                var res = await ScriptExec.start(Resolver.value, this.Engine, tm.ScriptText, gl, $"{ServiceName}-{method}");
                if (res.IsOk) {
                    return (res.ResultValue, responseType);
                }

                return (res.ErrorValue.ToString(), responseType);

            }

            throw new RpcException(new Status(StatusCode.NotFound, $"Method {method} not found"));
        }

        public void TryAddMethod(MethodInfo methodInfo, string scriptText) {
            var tm = Methods.FirstOrDefault(m => m.MethodInfo.Name == methodInfo.Name);
            if (tm == null) {
                Methods.Add(new TargetMethod(methodInfo, scriptText));
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