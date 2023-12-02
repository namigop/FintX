namespace Tefin.ViewModels.Explorer;

public class FileNode : NodeBase {

    public FileNode(string fullPath) {
        this.CanOpen = true;
        this.FullPath = fullPath;
        this.Title = Path.GetFileName(fullPath);
    }

    public string FullPath { get; }

    public override void Init() {
    }
}