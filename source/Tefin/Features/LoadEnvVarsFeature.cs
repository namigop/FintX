using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.Features;

public class LoadEnvVarsFeature {
    public GroupEnvConfigData LoadProjectEnvVars(string projectPath, IOs io) {
        var envVars =  VarsStructure.getVarsForProject(io, projectPath);
        return envVars ?? new GroupEnvConfigData(projectPath, []);
    }
    public Tuple<string, EnvConfigData> LoadProjectEnvVarsForEnv(string projectPath, IOs io, string envName) {
        var envVars =  VarsStructure.getVarsForProjectEnv(io, projectPath, envName);
        return envVars;
    }
    public GroupEnvConfigData LoadClientEnvVars(string clientPath, IOs io) {
        var envVars =  VarsStructure.getVarsForClient(io, clientPath);
        return envVars ?? new GroupEnvConfigData(clientPath, []);
    }
    
    public Tuple<string, EnvConfigData> LoadClientEnvVarsForEnv(string clientPath, IOs io, string envName) {
        var envVars =  VarsStructure.getVarsForClientEnv(io, clientPath, envName);
        return envVars;
    }
}