using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class ReadClientConfigFeature(string clientConfigFile, IOResolver io) {

    public ProjectTypes.ClientConfig Read() {
        var json = io.File.ReadAllText(clientConfigFile);
        return Core.Utils.jsonDeserialize<ProjectTypes.ClientConfig>(json);
    }
}