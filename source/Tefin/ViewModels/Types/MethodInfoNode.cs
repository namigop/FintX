#region

using System.Reflection;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Grpc;
using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels.Types;

public sealed class MethodInfoNode : NodeBase {
    public MethodInfoNode(MethodInfo mi) {
        this.IsExpanded = true;
        this.CanOpen = true;
        this.Title = mi.Name;
        this.SubTitle = $"{{{GrpcMethod.getMethodType(mi)}}}";
    }

    public List<RequestVariable> Variables { get; } = [];
    
    public override void Init() {
    }
    public void TryUpdateTemplatedChildNodes() {
        if (this.Variables.Count == 0 )
            return;
        
        var templatedNodes =
            this.FindChildNodes(n => n is SystemNode sn && !string.IsNullOrWhiteSpace(sn.EnvVarTag))
                .Cast<SystemNode>();

        var envFile = Current.EnvFilePath;
        if (string.IsNullOrWhiteSpace(envFile))
            return;

        var envVars = VarsStructure.getVars(this.Io, Current.ProjectPath);
        var current = envVars.Variables.FirstOrDefault(t => t.Item1 == Current.EnvFilePath);
        if (current == null)
            return;

        foreach (var node in templatedNodes) {
            foreach (var v in current.Item2.Variables) {
                var tagName = v.Name;
                if (node.EnvVarTag == tagName) {
                    var varValue = GetValueOrDefault2(v.CurrentValue, v.DefaultValue, node.Type, this.Io);
                    node.Value = varValue;
                    break;
                    //node.Value = v.Value;
                }

            }
        }

        static object GetValueOrDefault2(string vCurrentValue, string vDefaultValue, Type actualType, IOs io) {
            try {
                var cur = TypeHelper.indirectCast(vCurrentValue, actualType);
                if (cur != null)
                    return cur;

                var def = TypeHelper.indirectCast(vDefaultValue, actualType);
                if (def != null)
                    return def;

                return TypeBuilder.getDefault(actualType, true, Core.Utils.none<object>(), 0).Item2;
            }
            catch (Exception exc) {
                io.Log.Warn($"Unable get value for env variable. Exception: {exc}");
                return TypeBuilder.getDefault(actualType, true, Core.Utils.none<object>(), 0).Item2;
            }

        }
    }
}