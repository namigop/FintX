#region

using Tefin.Core;
using Tefin.Core.Interop;

#endregion

namespace Tefin.Features;

public class SaveClientConfigFeature(string clientConfigFile, ProjectTypes.ClientConfig cfg, IOResolver io) {
    public async Task Save() => await Project.updateClientConfig(io, clientConfigFile, cfg);
}