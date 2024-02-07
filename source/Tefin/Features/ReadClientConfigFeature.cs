#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class ReadClientConfigFeature(string clientConfigFile, IOs io) {
    public ProjectTypes.ClientConfig Read() {
        var json = io.File.ReadAllText(clientConfigFile);
        return Instance.jsonDeserialize<ProjectTypes.ClientConfig>(json);
    }
}