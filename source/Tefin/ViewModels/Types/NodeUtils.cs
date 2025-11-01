using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels.Types;

public static class NodeUtils {
    public static void InitVariableNodes(this IExplorerItem root, List<VarDefinition> reqVars, string clientPath,
        IOs io) {
        //----------------------------------
        //setup the templated {{TAG}} nodes
        //----------------------------------
        var load = new LoadEnvVarsFeature();
        foreach (var reqVar in reqVars) {
            var envVar = load.FindEnvVar(clientPath, Current.Env, reqVar.Tag, io);
            if (envVar is null) {
                continue;
            }

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
                tsNode.EnvVar.CreateEnvVariable(reqVar.Tag, reqVar.JsonPath, envVar?.CurrentValue);
            }
        }
    }

    public static void TryUpdateTemplatedChildNodes(this MethodInfoNode root, IOs io) =>
        root.TryUpdateTemplatedChildNodes(root.ClientGroup.Path, root.Variables, io);

    public static void TryUpdateTemplatedChildNodes(this StreamNode root, IOs io) =>
        root.TryUpdateTemplatedChildNodes(root.ClientPath, root.Variables, io);

    public static void TryUpdateTemplatedChildNodes(this ResponseNode root, IOs io) =>
        root.TryUpdateTemplatedChildNodes(root.ClientPath, root.Variables, io);

    public static void TryUpdateTemplatedChildNodes(this IExplorerItem root, string clientPath,
        List<VarDefinition> variables, IOs io) {
        if (variables.Count == 0) {
            return;
        }

        var envFile = Current.EnvFilePath;
        if (string.IsNullOrWhiteSpace(envFile)) {
            return;
        }

        var templatedNodes =
            root.FindChildNodes(n => {
                    if (n is SystemNode sn) {
                        return !string.IsNullOrWhiteSpace(sn.EnvVar.EnvVarTag);
                    }

                    if (n is TimestampNode tn) {
                        return !string.IsNullOrWhiteSpace(tn.EnvVar.EnvVarTag);
                    }

                    return false;
                })
                .Select(n => new {
                    IsSystemNode = n is SystemNode,
                    SystemNode = n as SystemNode,
                    TimestampNode = n as TimestampNode,
                    ((TypeBaseNode)n).Type
                })
                .ToArray();

        var load = new LoadEnvVarsFeature();
        foreach (var node in templatedNodes) {
            var envVar = load.FindEnvVar(
                clientPath,
                Current.Env,
                node.IsSystemNode ? node.SystemNode!.EnvVar.EnvVarTag : node.TimestampNode.EnvVar.EnvVarTag,
                io);
            if (envVar == null) {
                continue;
            }

            var value = GetValueOrDefault(envVar.CurrentValue, envVar.DefaultValue, node.Type, io);
            if (node.IsSystemNode) {
                node.SystemNode!.Value = value;
            }
            else {
                node.TimestampNode.Value = value;
            }
        }

        static object GetValueOrDefault(string vCurrentValue, string vDefaultValue, Type actualType, IOs io) {
            try {
                var cur = TypeHelper.indirectCast(vCurrentValue, actualType);
                if (cur != null) {
                    return cur;
                }

                var def = TypeHelper.indirectCast(vDefaultValue, actualType);
                if (def != null) {
                    return def;
                }

                return TypeBuilder.getDefault(actualType, true, Core.Utils.none<object>(), 0).Item2;
            }
            catch (Exception exc) {
                io.Log.Warn($"Unable get value for env variable. Exception: {exc}");
                return TypeBuilder.getDefault(actualType, true, Core.Utils.none<object>(), 0).Item2;
            }
        }
    }
}