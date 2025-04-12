using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadEnvVarsFeature {
    public ProjectEnvConfigData Run(ProjectTypes.Project proj, IOs io) {
        var envVars =  VarsStructure.getVarsForProject(io, proj.Path);
        return envVars ?? new ProjectEnvConfigData([]);
    }
}