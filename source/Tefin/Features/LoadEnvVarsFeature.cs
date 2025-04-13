using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadEnvVarsFeature {
    public GroupEnvConfigData LoadProjectEnvVars(string projectPath, IOs io) {
        var envVars =  VarsStructure.getVarsForProject(io, projectPath);
        return envVars ?? new GroupEnvConfigData(projectPath, []);
    }
    public GroupEnvConfigData LoadClientEnvVars(string clientPath, IOs io) {
        var envVars =  VarsStructure.getVarsForClient(io, clientPath);
        return envVars ?? new GroupEnvConfigData(clientPath, []);
    }
}