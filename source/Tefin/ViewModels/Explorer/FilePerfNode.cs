namespace Tefin.ViewModels.Explorer;

public class FilePerfNode : FileNode {
    public FilePerfNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
    }
}