using System.Timers;

using Tefin.Core;
using Tefin.Core.Infra.Actors;
using Tefin.Core.Interop;
using Tefin.Messages;

namespace Tefin.Features;

public class MonitorChangesFeature(IOs io) {
    private static FileSystemWatcher? _watcher;
    private static readonly object _lock = new();
    private readonly Dictionary<string, Timer?> _changedFiles = new();

    private void OnChanged(object sender, FileSystemEventArgs e) {
        if (e.Name == ProjectTypes.ProjectSaveState.FileName) {
            return;
        }

        if (!this._changedFiles.TryGetValue(e.FullPath, out var timer)) {
            timer = new Timer(TimeSpan.FromMilliseconds(500));
            timer.AutoReset = false;
            timer.Elapsed += (_, _) => {
                this._changedFiles.Remove(e.FullPath);
                io.Log.Info($"File changed: {e.Name}");
                var msg = new FileChangeMessage(e.FullPath, "", e.ChangeType);
                GlobalHub.publish(msg);
            };

            this._changedFiles.Add(e.FullPath, timer);
            timer.Start();
        }
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

    public void Run(ProjectTypes.Project project) {
        lock (_lock) {
            //Whenever Run is called, we dispose of the old one, essentially
            //just monitoring one folder at a time.
            _watcher?.Dispose();
            var watcher = new FileSystemWatcher(project.Path);
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            watcher.Error += this.OnError;
            watcher.Renamed += this.OnRenamed;
            watcher.Deleted += this.OnDeleted;
            watcher.Created += this.OnCreated;
            watcher.Changed += this.OnChanged;

            _watcher = watcher;
        }
    }
}