using Tefin.Core;
using Tefin.ViewModels.Types;

namespace Tefin.Features;

public class SaveEnvVarsFeature() {
    
    /// <summary>
    /// Save to the var/{ENV}.fxv file. The .fxv file contains the Tags and default/current values 
    /// </summary>
    public void Save(RequestVariable v, string defaultValueAsText, string clientPath, string currentEnv, IOs io) {
        var load = new LoadEnvVarsFeature();
        if (v.Scope == RequestEnvVarScope.Client) {
            var (envFile, envConfigData) = load.LoadClientEnvVarsForEnv(clientPath, io, currentEnv);
            var defaultValue = defaultValueAsText;
            
            var curVar = envConfigData.Variables.FirstOrDefault(c => c.Name == v.Tag);
            if (curVar != null)
                envConfigData.Variables.Remove(curVar);
            
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", v.TypeName);
            envConfigData.Variables.Add(clientVar);
            VarsStructure.saveToEnvFile(io, envFile, envConfigData);
        }
        
        if (v.Scope == RequestEnvVarScope.Project) {
            var projectPath = io.Dir.GetDirectoryName(clientPath);
            var (envFile, envConfigData) = load.LoadProjectEnvVarsForEnv(projectPath, io, currentEnv);
            var defaultValue = defaultValueAsText;
            var curVar = envConfigData.Variables.FirstOrDefault(c => c.Name == v.Tag);
            if (curVar != null)
                envConfigData.Variables.Remove(curVar);
            
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", v.TypeName);
            envConfigData.Variables.Add(clientVar);
            VarsStructure.saveToEnvFile(io, envFile, envConfigData);
        }
    }
}