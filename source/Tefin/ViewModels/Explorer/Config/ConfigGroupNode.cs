using Tefin.Core.Interop;
using Tefin.Features;
using Tefin.ViewModels.Explorer.Client;

namespace Tefin.ViewModels.Explorer.Config;

public class ConfigGroupNode : ExplorerRootNode {
    private readonly string _projectPath;
    
    public ConfigGroupNode(string projectPath) : base() {
        this._projectPath = projectPath;
    }



    public override void Init() {
        //Load env files
        var load = new LoadEnvVarsFeature();
        var projectEnvData = load.LoadProjectEnvVars(this._projectPath, this.Io);
        
        foreach (var (fullPath, env) in projectEnvData.Variables) {
            var node = new EnvNode(fullPath) { SubTitle = env.Description};
            this.Items.Add(node);
        }

    }
}