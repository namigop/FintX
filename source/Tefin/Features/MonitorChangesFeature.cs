using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;

namespace Tefin.Features;

public class MonitorChangesFeature(IOResolver io) {
    private static FileSystemWatcher watcher;
    public void Run(ProjectTypes.Project project) {
        watcher?.Dispose();
        watcher = new FileSystemWatcher(project.Path);
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        watcher.Error += this.OnError;
        watcher.Renamed += this.OnRenamed;
        watcher.Deleted += this.OnDeleted;
        watcher.Created += this.OnCreated;
    }

    private void OnCreated(object sender, FileSystemEventArgs e) {
        io.Log.Info($"File created: {e.Name}");
        var msg = new FileChangeMessage(e.FullPath, "", e.ChangeType);
        GlobalHub.publish(msg);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e) {
        io.Log.Info($"File deleted: {e.Name}");
        var msg = new FileChangeMessage(e.FullPath, "", e.ChangeType);
        GlobalHub.publish(msg);
    }

    private void OnRenamed(object sender, RenamedEventArgs e) {
        io.Log.Info($"File renamed from \"{e.OldName}\" to \"{e.Name}\"");
        var msg = new FileChangeMessage(e.FullPath, e.OldFullPath, e.ChangeType);
        GlobalHub.publish(msg);
    }

    private void OnError(object sender, ErrorEventArgs e) {
        io.Log.Warn(e.GetException().ToString());
    }
}