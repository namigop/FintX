using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class ReadServiceMockConfigFeature(string mockConfigFile, IOs io) {
    public ProjectTypes.ServiceMockConfig Read() {
        var json = io.File.ReadAllText(mockConfigFile);
        return Instance.jsonDeserialize<ProjectTypes.ServiceMockConfig>(json);
    }
}