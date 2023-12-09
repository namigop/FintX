using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class ReadClientConfigFeature {
    private readonly string _clientConfigFile;
    private readonly IOResolver _io;
    public ReadClientConfigFeature(string clientConfigFile, IOResolver io) {
        this._clientConfigFile = clientConfigFile;
        this._io = io;
    }

    public ProjectTypes.ClientConfig Read() {
        var json = this._io.File.ReadAllText(this._clientConfigFile);
        return Instance.jsonDeserialize<ProjectTypes.ClientConfig>(json);
    }
}