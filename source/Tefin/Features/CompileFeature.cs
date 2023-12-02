#region

using Tefin.Core;
using Tefin.Core.Build;
using Tefin.Grpc;

#endregion

namespace Tefin.Features;

public class CompileFeature(string serviceName, string clientName, string description, string[] protoFiles, string reflectionUrl, IOResolver io) {

    public async Task<(bool, CompileOutput)> CompileExisting(string[] csFiles) {
        CompileParameters? cParams = new(clientName, description, serviceName, protoFiles, Array.Empty<string>(), reflectionUrl, null);
        var csFilesRet = Res.ok(csFiles);
        var com = await Grpc.Features.compile(csFilesRet, cParams);
        if (com.IsOk) {
            return (true, com.ResultValue);
        }

        io.Log.Error(com.ErrorValue);
        return (false, com.ResultValue);
    }

    public async Task<(bool, CompileOutput)> Run() {
        var csFiles = Array.Empty<string>();
        var cParams = new CompileParameters(clientName, description, serviceName, protoFiles, csFiles, reflectionUrl, config: null);
        var csFilesRet = await Grpc.Features.generateSourceFiles(cParams);
        var com = await Grpc.Features.compile(csFilesRet, cParams);
        if (com.IsOk) {
            return (true, com.ResultValue);
        }

        io.Log.Error(com.ErrorValue);
        return (false, com.ResultValue);
    }
}