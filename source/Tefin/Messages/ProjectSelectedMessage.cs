namespace Tefin.Messages;

public class ProjectSelectedMessage(string projName,  string projectPath, string package) : MessageBase {
    public string ProjectName { get; } = projName;
    public string ProjectPath { get; } = projectPath;
    public string Package { get; } = package;
}