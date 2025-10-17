using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class SaveServiceMockConfigFeature(string mockConfigFile, ProjectTypes.ServiceMockConfig cfg, IOs io) {
    public async Task Save() => await ServiceMockStructure.updateConfig(io, mockConfigFile, cfg);
}