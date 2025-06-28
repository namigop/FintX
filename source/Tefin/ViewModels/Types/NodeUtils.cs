using Tefin.Core;
using Tefin.Features;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Types;

public static class NodeUtils {
    public static void InitVariableNodes(this IExplorerItem root, List<RequestVariable> reqVars, string clientPath, IOs io) {
        if (reqVars == null)
            return;
        
        //----------------------------------
        //setup the templated {{TAG}} nodes
        //----------------------------------
        var load = new LoadEnvVarsFeature();
        //var methodInfoNode = (MethodInfoNode)this.Items[0];
        foreach (var reqVar in reqVars) {
            var envVar = load.FindEnvVar(clientPath, Current.Env, reqVar.Tag, io);
            if (envVar is null)
                continue;

            var node = root.FindChildNode(i => {
                if (i is SystemNode sn) {
                    var pathToRoot = sn.GetJsonPath();
                    return pathToRoot == reqVar.JsonPath;
                }

                if (i is TimestampNode tn) {
                    var pathToRoot = tn.GetJsonPath();
                    return pathToRoot == reqVar.JsonPath;
                }

                return false;
            });

            if (node is SystemNode sysNode) {
                sysNode.EnvVar.CreateEnvVariable(reqVar.Tag, reqVar.JsonPath, envVar?.CurrentValue);
            }

            if (node is TimestampNode tsNode) {
                tsNode.EnvVar.CreateEnvVariable(reqVar.Tag, reqVar.JsonPath);
            }
        }
    }
}