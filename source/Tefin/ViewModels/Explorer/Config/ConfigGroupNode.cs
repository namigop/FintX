using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer.Client;

namespace Tefin.ViewModels.Explorer.Config;

public class ConfigGroupNode : ExplorerRootNode {
    public ConfigGroupNode(ProjectTypes.ClientGroup cg, Type? clientType) : base(cg, clientType) {
        throw new NotImplementedException();
    }
    public ConfigGroupNode() : base() {
    }

 

    public override void Init() { }
}