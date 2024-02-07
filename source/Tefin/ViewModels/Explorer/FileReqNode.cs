using System.Windows.Input;

namespace Tefin.ViewModels.Explorer;

public class FileReqNode : FileNode {
    public FileReqNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
        this.ExportCommand = this.CreateCommand(this.OnExport);
    }

    public ICommand ExportCommand { get; }

    public override string Title {
        get => base.Title;
        set {
            base.Title = value;
            this.TempTitle = value;
        }
    }

    private void OnExport() => throw new NotImplementedException();

    public ClientMethodViewModelBase? CreateViewModel() => ((MethodNode)this.Parent!).CreateViewModel();
}