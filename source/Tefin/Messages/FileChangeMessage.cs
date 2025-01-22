namespace Tefin.Messages;

public class FileChangeMessage : MessageBase {
    public FileChangeMessage(string fullPath, string oldFullPath, WatcherChangeTypes changeType) {
        this.ChangeType = changeType;
        this.FullPath = fullPath;
        this.OldFullPath = oldFullPath;
    }

    public WatcherChangeTypes ChangeType { get; }
    public string FullPath { get; }

    public string OldFullPath { get; }
}