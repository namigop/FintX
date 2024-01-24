namespace Tefin.Messages;

public class FileChangeMessage(string fullPath, string oldFullPath, WatcherChangeTypes changeType) : MessageBase {
    public WatcherChangeTypes ChangeType { get; } = changeType;
    public string FullPath { get; } = fullPath;

    public string OldFullPath { get; } = oldFullPath;
}