namespace Tefin.Messages;

public class NewProjectCreatedMessage(string package, string projectPath) : MessageBase {
    public string Package { get; } = package;
    public string ProjectPath { get; } = projectPath;
}