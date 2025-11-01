using Tefin.Core;
using Tefin.ViewModels.Types;

namespace Tefin.Features;

public class RemoveEnvVarsFeature {
    public void Remove(VarDefinition v, string clientPath, string currentEnv, IOs io) {
        var load = new LoadEnvVarsFeature();
        if (v.Scope == RequestEnvVarScope.Client) {
            var (envFile, envConfigData) = load.LoadClientEnvVarsForEnv(clientPath, io, currentEnv);
            var curVar = envConfigData.Variables.FirstOrDefault(c => c.Name == v.Tag);
            if (curVar != null) {
                envConfigData.Variables.Remove(curVar);
                VarsStructure.saveToEnvFile(io, envFile, envConfigData);
            }
        }

        if (v.Scope == RequestEnvVarScope.Project) {
            var projectPath = io.Dir.GetDirectoryName(clientPath);
            var (envFile, envConfigData) = load.LoadProjectEnvVarsForEnv(projectPath, io, currentEnv);
            var curVar = envConfigData.Variables.FirstOrDefault(c => c.Name == v.Tag);
            if (curVar != null) {
                envConfigData.Variables.Remove(curVar);
                VarsStructure.saveToEnvFile(io, envFile, envConfigData);
            }
        }
    }
}