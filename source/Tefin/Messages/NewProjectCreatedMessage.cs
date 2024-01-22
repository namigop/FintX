namespace Tefin.Messages;

public class NewProjectCreatedMessage(string projectPath) : MessageBase {
    public string ProjectPath { get; } = projectPath;
}