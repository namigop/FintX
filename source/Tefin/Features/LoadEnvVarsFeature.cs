using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin.Features;

public class LoadEnvVarsFeature {
    private static readonly Dictionary<Env, EnvConfigData> _allEnvVars = new();
    static LoadEnvVarsFeature() => GlobalHub.subscribe<FileChangeMessage>(OnFileChange);

    public EnvVar? FindEnvVar(string clientPath, string envName, string tag, IOs io) {
        //check if env variable is in the client cache
        var clientVarFiles = VarsStructure.getVarFilesForClient(io, clientPath);
        foreach (var clientVarFile in clientVarFiles) {
            var key = new Env(clientVarFile);
            if (_allEnvVars.TryGetValue(key, out var cachedData)) {
                return cachedData.Variables.FirstOrDefault(t => t.Name == tag);
            }
        }

        //check if env variable is in the proj cache
        var projectVarFiles = VarsStructure.getVarFilesForProject(io, Path.GetDirectoryName(clientPath)!);
        foreach (var projVarFile in projectVarFiles) {
            var key = new Env(projVarFile);
            if (_allEnvVars.TryGetValue(key, out var cachedData)) {
                return cachedData.Variables.FirstOrDefault(t => t.Name == tag);
            }
        }

        //Check if it's a client env variable and load from disk
        var (envFile, envConfig) = this.LoadClientEnvVarsForEnv(clientPath, io, envName);
        var clientVar = envConfig.Variables.FirstOrDefault(t => t.Name == tag);
        if (clientVar != null) {
            return clientVar;
        }

        //Check if it's a proj env variable and load from disk
        var projectPath = Path.GetDirectoryName(clientPath)!;
        var (_, projectEnvConfig) = this.LoadProjectEnvVarsForEnv(projectPath, io, envName);
        var projectVar = projectEnvConfig.Variables.FirstOrDefault(t => t.Name == tag);
        if (projectVar != null) {
            return projectVar;
        }

        return null;
    }

    public GroupEnvConfigData LoadClientEnvVars(string clientPath, IOs io) {
        var envVars = VarsStructure.getVarsForClient(io, clientPath);
        return envVars ?? new GroupEnvConfigData(clientPath, []);
    }

    public (string envFile, EnvConfigData envConfigData) LoadClientEnvVarsForEnv(string clientPath, IOs io,
        string envName) {
        var (envFile, envConfigData) = VarsStructure.getVarsForClientEnv(io, clientPath, envName);
        _allEnvVars[new Env(envFile)] = envConfigData;
        return (envFile, envConfigData);
        ;
    }

    public EnvConfigData LoadEnvVarsFromFile(string envFile) {
        var key = new Env(envFile);
        if (!_allEnvVars.TryGetValue(key, out var envConfigData)) {
            return envConfigData!;
        }

        envConfigData = VarsStructure.getVarsFromFile(Resolver.value, envFile);
        _allEnvVars[new Env(envFile)] = envConfigData;
        return envConfigData;
    }

    public GroupEnvConfigData LoadProjectEnvVars(string projectPath, IOs io) {
        var envVars = VarsStructure.getVarsForProject(io, projectPath);
        if (envVars != null) {
            foreach (var (envFile, envConfigData) in envVars.Variables) {
                _allEnvVars[new Env(envFile)] = envConfigData;
            }

            return envVars;
        }

        return new GroupEnvConfigData(projectPath, []);
    }

    public (string envFile, EnvConfigData envConfigData) LoadProjectEnvVarsForEnv(string projectPath, IOs io,
        string envName) {
        var (envFile, envConfigData) = VarsStructure.getVarsForProjectEnv(io, projectPath, envName);
        _allEnvVars[new Env(envFile)] = envConfigData;
        return (envFile, envConfigData);
    }

    private static void OnFileChange(FileChangeMessage msg) {
        if (Path.GetExtension(msg.FullPath) == Ext.envExt) {
            var envFile = Path.GetFullPath(msg.FullPath);
            var envConfigData = VarsStructure.getVarsFromFile(Resolver.value, envFile);
            _allEnvVars[new Env(envFile)] = envConfigData;
        }
    }

    private record Env(string File);
}