#region

using Tefin.Core;
using Tefin.Grpc;

#endregion

namespace Tefin.Features;

public class DiscoverFeature(string[] protoFiles, string reflectionUrl) {

    public async Task<(bool, string[])> Discover(IOResolver io) {
        var ret = await ServiceClient.discover(io, new DiscoverParameters(protoFiles, new Uri(reflectionUrl)));
        if (ret.IsOk) {
            io.Log.Info($"Service discovery successful. {ret.ResultValue}");
        }
        else {
            io.Log.Error(ret.ErrorValue);
        }

        return (ret.IsOk, ret.ResultValue);
    }
}