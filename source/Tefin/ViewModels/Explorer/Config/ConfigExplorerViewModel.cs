using System.Windows.Input;

using Tefin.Core;
using Tefin.Core.Interop;

namespace Tefin.ViewModels.Explorer.Config;

public class ConfigExplorerViewModel : ExplorerViewModel<ConfigGroupNode> {
    public ConfigExplorerViewModel() {
        this.SupportedExtensions = [Ext.envExt];
        this.EditCommand = this.CreateCommand(this.OnEdit);
    }

    public ICommand EditCommand { get; }

    protected override string[] SupportedExtensions { get; }

    protected override MultiNodeFile CreateMultiNodeFile(IExplorerItem[] items, ProjectTypes.ClientGroup client) => throw new NotImplementedException();

    protected override NodeBase CreateMultiNodeFolder(IExplorerItem[] items, ProjectTypes.ClientGroup client) => throw new NotImplementedException();

    protected override string GetRootFilePath(string clientPath) => throw new NotImplementedException();
    protected override ConfigGroupNode CreateRootNode(ProjectTypes.ClientGroup cg, Type? type = null) => throw new NotImplementedException();

    private void OnEdit() { }

    public void Init() {
        var envGroup = new ConfigGroupNode { Title = "Environments", SubTitle = "All environments" };
        var devEnv = new EnvNode("TODOPath") { Title = "DEV", SubTitle = "Development environments" };
        var uatEnv = new EnvNode("TODOPath") { Title = "UAT", SubTitle = "UAT environments" };
        var prodEnv = new EnvNode("TODOPath") { Title = "PROD", SubTitle = "Production environments" };
        envGroup.AddItem(devEnv);
        envGroup.AddItem(uatEnv);
        envGroup.AddItem(prodEnv);
        this.Items.Add(envGroup);
    }
}