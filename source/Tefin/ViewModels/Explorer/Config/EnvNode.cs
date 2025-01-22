using System.Windows.Input;

namespace Tefin.ViewModels.Explorer.Config;

public class EnvNode : FileNode {
    public EnvNode(string fullPath) : base(fullPath) {
        this.ExportCommand = this.CreateCommand(this.OnExport);
    }


    public ICommand ExportCommand { get; }

    private Task OnExport() => throw new NotImplementedException();

    public override void Init() { }
}