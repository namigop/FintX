namespace Tefin.ViewModels.Explorer;

public class FileReqNode : FileNode {

    public FileReqNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
    }

    public override string Title {
        get => base.Title;
        set {
            base.Title = value;
            this.TempTitle = value;
        }
    }

    public ClientMethodViewModelBase? CreateViewModel() {
        return ((MethodNode)this.Parent).CreateViewModel();
    }
}