using Tefin.Core.Interop;
using Tefin.ViewModels.Explorer.Client;

namespace Tefin.ViewModels.Explorer.Config;

public class ConfigGroupNode : RootNode {
    public ConfigGroupNode(ProjectTypes.ClientGroup cg, Type? clientType) : base(cg, clientType) {
        
    }

    public override void Init() { }
}