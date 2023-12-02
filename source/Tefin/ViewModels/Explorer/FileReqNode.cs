namespace Tefin.ViewModels.Explorer;

public class FileReqNode : FileNode {

    public FileReqNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
    }
}