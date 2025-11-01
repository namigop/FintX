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

    public async Task<(bool, CompileOutput)>
        CompileExisting(string[] codeFiles, bool createMockService, bool recompile) {
        try {
            var key = string.Join("-", codeFiles);
            if (recompile) {
                CompilationCache.Remove(key);
            }

            if (CompilationCache.TryGetValue(key, out var cOutput)) {
                return (true, cOutput);
            }

            if (createMockService) {
                GlobalHub.publish(new ServiceMockCompileMessage(true));
            }
            else {
                GlobalHub.publish(new ClientCompileMessage(true));
            }

            CompileParameters? cParams = new(clientName, description, serviceName, protoFiles, [], reflectionUrl,
                recompile, null);
            var com = await ServiceClient.compile(io, codeFiles, cParams);
            if (com.IsOk) {
                CompilationCache.Add(key, com.ResultValue);
                return (true, com.ResultValue);
            }

            io.Log.Error(com.ErrorValue);
            return (false, com.ResultValue);
        }
        finally {
            if (createMockService) {
                GlobalHub.publish(new ServiceMockCompileMessage(false));
            }
            else {
                GlobalHub.publish(new ClientCompileMessage(false));
            }
        }
    }

    public async Task<(bool, CompileOutput)> Run(bool createMockService) {
        try {
            GlobalHub.publish(new ClientCompileMessage(true));
            var csFiles = Array.Empty<string>();
            var cParams = new CompileParameters(clientName, description, serviceName, protoFiles, csFiles,
                reflectionUrl, false, null);
            var csFilesRet = await ServiceClient.generateSourceFiles(io, cParams);
            if (!csFilesRet.IsOk) {
                io.Log.Error(csFilesRet.ErrorValue);
                return (false, null!);
            }

            if (createMockService) {
                var grpcFile = csFilesRet.ResultValue.First(t => ServiceMock.containsServiceBase(io, t));
                await ServiceMock.insertService(io, grpcFile);
            }

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
            if (!createMockService) {
                GlobalHub.publish(new ClientCompileMessage(false));
            }
        }
    }
}