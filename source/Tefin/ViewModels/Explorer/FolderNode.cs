namespace Tefin.ViewModels.Explorer;

public class FolderNode : NodeBase {

    public FolderNode(string dirPath) {
        this.IsExpanded = true;
        this.FullPath = dirPath;
        base.Title = Path.GetFileName(dirPath);
    }

    public string FullPath { get; private set; }

    public override void Init() {
    }
}