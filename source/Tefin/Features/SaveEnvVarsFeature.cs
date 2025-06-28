using DynamicData;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Types;

namespace Tefin.Features;

public class SaveEnvVarsFeature() {
    private static string[] DisplayTypes = SystemType.getTypesForDisplay();
    private static Type[] ActualTypes = SystemType.getTypes().ToArray();

    /// <summary>
    /// Save to the var/{ENV}.fxv file. The .fxv file contains the Tags and default/current values 
    /// </summary>
    public void Save(VarDefinition v, string defaultValueAsText, string clientPath, string currentEnv, IOs io) {
        
        var actualType = ActualTypes.FirstOrDefault(t => t.FullName == v.TypeName);
        var displayType = DisplayTypes[ActualTypes.IndexOf(actualType)];
        
        var load = new LoadEnvVarsFeature();
        if (v.Scope == RequestEnvVarScope.Client) {
            var (envFile, envConfigData) = load.LoadClientEnvVarsForEnv(clientPath, io, currentEnv);
            var defaultValue = defaultValueAsText;
            
            var curVar = envConfigData.Variables.FirstOrDefault(c => c.Name == v.Tag);
            if (curVar != null)
                envConfigData.Variables.Remove(curVar);
            
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", displayType);
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
            
            var clientVar = EnvConfig.createVar(v.Tag, defaultValue, defaultValue, "", displayType);
            envConfigData.Variables.Add(clientVar);
            VarsStructure.saveToEnvFile(io, envFile, envConfigData);
        }
    }
}