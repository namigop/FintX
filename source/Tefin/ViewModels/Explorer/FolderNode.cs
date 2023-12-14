namespace Tefin.ViewModels.Explorer;

public class FolderNode : NodeBase {
    public FolderNode(string dirPath) {
        this.IsExpanded = true;
        this.FullPath = dirPath;
        this.Title = Path.GetFileName(dirPath) ?? string.Empty;
    }

    public string FullPath { get; private set; }

    public override void Init() {
    }
}