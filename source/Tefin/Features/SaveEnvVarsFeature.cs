using Tefin.Core;

namespace Tefin.Features;

public class SaveEnvVarsFeature() {
    public void SaveProjectEnvVars(string projPath, EnvConfigData envConfigData, IOs io) {
        VarsStructure.saveEnvForProject(io, envConfigData, projPath);
    }
    public void SaveClientEnvVars(string clientPath, EnvConfigData envConfigData, IOs io) {
        VarsStructure.saveEnvForClient(io, envConfigData, clientPath);
    }
}