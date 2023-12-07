using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class SaveClientConfigFeature {
    private readonly string _clientConfigFile;
    private readonly ProjectTypes.ClientConfig _cfg;
    private readonly IOResolver _io;
    public SaveClientConfigFeature(string clientConfigFile, ProjectTypes.ClientConfig cfg, IOResolver io) {
        this._clientConfigFile = clientConfigFile;
        this._cfg = cfg;
        this._io = io;
    }

    public async Task Save() {
        await Project.updateClientConfig(this._io, this._clientConfigFile, this._cfg);
    }
}