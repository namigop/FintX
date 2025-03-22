using System.Windows.Input;

using Tefin.Core;

namespace Tefin.ViewModels.Explorer.Config;

public class EnvNode : FileNode {
    public EnvNode(string fullPath) : base(fullPath) {
        this.ExportCommand = this.CreateCommand(this.OnExport);
        this.EnvFile = fullPath;
        //this.EnvData = data;
        base.Title = Path.GetFileName(fullPath);
    }
    public string EnvFile { get; init; }

    public EnvConfigData GetEnvData() {
        return VarsStructure.getVarsFromFile(this.Io, this.EnvFile);
    }

    public ICommand ExportCommand { get; }

    private Task OnExport() => throw new NotImplementedException();

    public override void Init() { }
}