using Tefin.Core;

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

    public EnvVar? FindEnvVar(string clientPath, string envName,  string tag, IOs io) {
        //first check if it's a client env variable
        var (_, envConfig) = LoadClientEnvVarsForEnv(clientPath, io, envName);
        var clientVar = envConfig.Variables.FirstOrDefault(t => t.Name == tag);
        if (clientVar != null) {
            return clientVar;
        }
        
        //then check if it's a project env variable
        var projectPath = Path.GetDirectoryName(clientPath)!;
        var (_, projectEnvConfig) = LoadProjectEnvVarsForEnv(projectPath, io, envName);
        var projectVar = projectEnvConfig.Variables.FirstOrDefault(t => t.Name == tag);
        if (projectVar != null) {
            return projectVar;
        }
        return null;
    }
}