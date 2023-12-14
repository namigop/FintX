namespace Tefin.ViewModels.Explorer;

public class FileTestNode : FileNode {
    public FileTestNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
    }
}