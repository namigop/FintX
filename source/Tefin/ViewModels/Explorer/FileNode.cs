namespace Tefin.ViewModels.Explorer;

public class FileNode : NodeBase {
    public FileNode(string fullPath) {
        this.CanOpen = true;
        this.FullPath = fullPath;
        this.Title = Path.GetFileName(fullPath);
    }

    public string FullPath { get; private set; }

    public override void Init() {
    }

    public void UpdateFilePath(string newFilePath) {
        if (Io.File.Exists(newFilePath)) {
            this.FullPath = newFilePath;
            this.Title = Path.GetFileName(newFilePath);
        }
        else {
            Io.Log.Warn($"Unable to update file path. {newFilePath} does not exist");
        }
    }
}