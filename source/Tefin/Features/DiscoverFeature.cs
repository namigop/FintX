using Tefin.Core;
using Tefin.Grpc;

namespace Tefin.Features;

public class DiscoverFeature {
    private readonly string[] _protoFiles;
    private readonly string _reflectionUrl;
    public DiscoverFeature(string[] protoFiles, string reflectionUrl) {
        this._protoFiles = protoFiles;
        this._reflectionUrl = reflectionUrl;
    }

    public async Task<(bool, string[])> Discover(IOResolver io) {
        var ret = await Grpc.Features.discover(new DiscoverParameters(this._protoFiles, new Uri(this._reflectionUrl)));
        if (ret.IsOk) {
            io.Log.Info($"Service discovery successful. {ret.ResultValue}");
        }
        else {
            io.Log.Error(ret.ErrorValue);
        }

        return (ret.IsOk, ret.ResultValue);
    }
}