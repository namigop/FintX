namespace Tefin.Messages;

public class EnvSelectedMessage(string environmentName,  string envFilePath) : MessageBase {
    public string EnvFilePath { get; } = envFilePath;
    public string EnvironmentName { get; } = environmentName;
}