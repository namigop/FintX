using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;

namespace Tefin.Features;

public class MonitorChangesFeature(IOs io) {
    private static FileSystemWatcher? _watcher;

    public void Run(ProjectTypes.Project project) {
        //Whenever Run is called we dispose of the old one -essentially
        //just monitoring one folder at a time.
        _watcher?.Dispose();
        _watcher = new FileSystemWatcher(project.Path);
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
        _watcher.Error += this.OnError;
        _watcher.Renamed += this.OnRenamed;
        _watcher.Deleted += this.OnDeleted;
        _watcher.Created += this.OnCreated;
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

    private void OnError(object sender, ErrorEventArgs e) => io.Log.Warn(e.GetException().ToString());

    private void OnRenamed(object sender, RenamedEventArgs e) {
        io.Log.Info($"File renamed from \"{e.OldName}\" to \"{e.Name}\"");
        var msg = new FileChangeMessage(e.FullPath, e.OldFullPath, e.ChangeType);
        GlobalHub.publish(msg);
    }
}