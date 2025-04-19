using Tefin.Core;
using Tefin.ViewModels.Types;

namespace Tefin.Features;

public class SaveEnvVarsFeature() {
 
    public void Save(RequestVariable v, string formattedValue, string path, string currentEnv, IOs io) {
        var load = new LoadEnvVarsFeature();
        if (v.Scope == RequestEnvVarScope.Client) {
            var (envFile, envConfigData) = load.LoadClientEnvVarsForEnv(path, io, currentEnv);
            var defaultValue = formattedValue;
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", v.TypeName);
            envConfigData.Variables.Add(clientVar);
            VarsStructure.saveToEnvFile(io, envFile, envConfigData);
        }
        
        if (v.Scope == RequestEnvVarScope.Project) {
            var (envFile, envConfigData) = load.LoadProjectEnvVarsForEnv(path, io, currentEnv);
            var defaultValue = formattedValue;
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", v.TypeName);
            envConfigData.Variables.Add(clientVar);
            VarsStructure.saveToEnvFile(io, envFile, envConfigData);
        }
    }
}