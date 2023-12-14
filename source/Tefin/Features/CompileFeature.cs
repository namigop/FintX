#region

using Tefin.Core;
using Tefin.Core.Build;
using Tefin.Grpc;

#endregion

namespace Tefin.Features;

public class CompileFeature {
    private readonly string _clientName;
    private readonly string _description;
    private readonly IOResolver _io;
    private readonly string[] _protoFiles;
    private readonly string _reflectionUrl;
    private readonly string _serviceName;
    public CompileFeature(string serviceName, string clientName, string description, string[] protoFiles, string reflectionUrl, IOResolver io) {
        this._serviceName = serviceName;
        this._clientName = clientName;
        this._description = description;
        this._protoFiles = protoFiles;
        this._reflectionUrl = reflectionUrl;
        this._io = io;
    }

    public async Task<(bool, CompileOutput)> CompileExisting(string[] csFiles) {
        CompileParameters? cParams = new(this._clientName, this._description, this._serviceName, this._protoFiles, Array.Empty<string>(), this._reflectionUrl, null);
        var com = await ServiceClient.compile(this._io, csFiles, cParams);
        if (com.IsOk) {
            return (true, com.ResultValue);
        }

        this._io.Log.Error(com.ErrorValue);
        return (false, com.ResultValue);
    }

    public async Task<(bool, CompileOutput)> Run() {
        var csFiles = Array.Empty<string>();
        var cParams = new CompileParameters(this._clientName, this._description, this._serviceName, this._protoFiles, csFiles, this._reflectionUrl, null);
        var csFilesRet = await ServiceClient.generateSourceFiles(this._io, cParams);
        var com = await ServiceClient.compile(Resolver.value, csFilesRet.ResultValue, cParams);
        if (com.IsOk) {
            return (true, com.ResultValue);
        }

        this._io.Log.Error(com.ErrorValue);
        return (false, com.ResultValue);
    }
}