namespace Tefin.Messages;

public class FileChangeMessage(string fullPath, string oldFullPath, WatcherChangeTypes changeType) : MessageBase {
    public string FullPath { get; } = fullPath;

    public string OldFullPath { get; } = oldFullPath;

    public WatcherChangeTypes ChangeType { get; } = changeType;
}