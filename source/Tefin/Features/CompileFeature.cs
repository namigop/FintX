#region

using Tefin.Core;
using Tefin.Core.Build;
using Tefin.Core.Infra.Actors;
using Tefin.Grpc;
using Tefin.Messages;

#endregion

namespace Tefin.Features;

public class CompileFeature(
    string serviceName,
    string clientName,
    string description,
    string[] protoFiles,
    string reflectionUrl,
    IOs io) {
    private static readonly Dictionary<string, CompileOutput> CompilationCache = new();

    public async Task<(bool, CompileOutput)> CompileExisting(string[] codeFiles) {
        try {
            var key = string.Join("-", codeFiles);
            if (CompilationCache.TryGetValue(key, out var cOutput)) {
                return (true, cOutput);
            }

            GlobalHub.publish(new ClientCompileMessage(true));
            CompileParameters? cParams = new(clientName, description, serviceName, protoFiles, [],
                reflectionUrl, null);
            var com = await ServiceClient.compile(io, codeFiles, cParams);
            if (com.IsOk) {
                CompilationCache.Add(key, com.ResultValue);
                return (true, com.ResultValue);
            }

            var output = com.ResultValue;
            if (!output.Success) {
                var err = String.Join(Environment.NewLine, output.CompilationErrors);
                io.Log.Error(err);
            }

            io.Log.Error(com.ErrorValue);
            return (false, com.ResultValue);
        }
        finally {
            GlobalHub.publish(new ClientCompileMessage(false));
        }
    }

    public async Task<(bool, CompileOutput)> Run() {
        try {
            GlobalHub.publish(new ClientCompileMessage(true));
            var csFiles = Array.Empty<string>();
            var cParams = new CompileParameters(clientName, description, serviceName, protoFiles, csFiles,
                reflectionUrl, null);
            var csFilesRet = await ServiceClient.generateSourceFiles(io, cParams);
            var com = await ServiceClient.compile(Resolver.value, csFilesRet.ResultValue, cParams);
            if (com.IsOk) {
                var key = string.Join("-", csFilesRet.ResultValue);
                CompilationCache[key] = com.ResultValue;
                return (true, com.ResultValue);
            }

            io.Log.Error(com.ErrorValue);
            return (false, com.ResultValue);
        }
        finally {
            GlobalHub.publish(new ClientCompileMessage(false));
        }
    }
}