using Tefin.Core.Infra.Actors;
using Tefin.Messages;

namespace Tefin;

public static class Current {
    static Current() {
        GlobalHub.subscribe<EnvSelectedMessage>(OnEnvSelected);
        GlobalHub.subscribe<ProjectSelectedMessage>(OnProjectSelected);
    }

    public static string Package { get; private set; } = "";

    public static string ProjectPath { get; private set; } = "";

    public static string ProjectName { get; private set; } = "";

    public static string EnvFilePath { get; private set; } = "";

    public static string Env { get; private set; } = "";

    private static void OnProjectSelected(ProjectSelectedMessage obj) {
        ProjectName = obj.ProjectName;
        ProjectPath = obj.ProjectPath;
        Package = obj.Package;
    }

    private static void OnEnvSelected(EnvSelectedMessage obj) {
        Env = obj.EnvironmentName;
        EnvFilePath = obj.EnvFilePath;
    }

    public static void Init() {
       //intentionally empty
    }
}