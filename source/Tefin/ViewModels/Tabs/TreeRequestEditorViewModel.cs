#region

using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeNodeBuilders;

using TypeInfo = Tefin.ViewModels.Types.TypeInfo;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TreeRequestEditorViewModel : ViewModelBase, IRequestEditorViewModel {
    public TreeRequestEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        //this.MethodParameterInstances = methodParameterInstances ?? new List<object?>();

        this.MethodInfo = methodInfo;
        this.MethodTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate",
                        "CellEditTemplate", //edittemplate
                        new GridLength(1, GridUnitType.Star)),
                    x => x.Items,
                    x => x.Items.Any(),
                    x => x.IsExpanded)
            }
        };

        this.Items.Add(new EmptyNode());
    }

    public ObservableCollection<IExplorerItem> Items { get; } = [];

    public HierarchicalTreeDataGridSource<IExplorerItem> MethodTree { get; }

    public CancellationTokenSource? CtsReq {
        get;
        private set;
    }

    public MethodInfo MethodInfo {
        get;
    }

    public void EndRequest() => this.CtsReq = null;

    //public List<object?> MethodParameterInstances { get; }
    public (bool, object?[]) GetParameters() {
        if (this.Items.Count == 0 || this.Items[0].Items.Count == 0) {
            return (false, []);
        }

        // check for nodes with env tag
        this.TryUpdateValueFromEnv();
        
        var mParams = this.Items[0].Items.Select(t => ((TypeBaseNode)t).Value).ToArray()!;
        var last = mParams.Last();
       
        if (last is CancellationToken) {
            if (CtsReq == null) {
                this.CtsReq = new CancellationTokenSource();
            }
            //replace the CancellationToken with one that we can cancel
            mParams[^1] = this.CtsReq.Token;
        }

        return (mParams.Any(), mParams);
    }

    private void TryUpdateValueFromEnv() {
        if (this.Items[0] is MethodInfoNode { Variables.Count: 0 })
            return;
        
        var templatedNodes = this.Items[0]
            .FindChildNodes(n => n is SystemNode sn && !string.IsNullOrWhiteSpace(sn.EnvVarTag))
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


    public void Show(object?[] parameters, RequestEnvVar[] envVars) {
        this.Items.Clear();
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        var methodNode = new MethodInfoNode(this.MethodInfo);
        this.Items.Add(methodNode);

        var counter = 0;
        foreach (var paramInfo in methodParams) {
            var instance = hasValues
                ? parameters[counter]
                : TypeBuilder.getDefault(paramInfo.ParameterType, true, Core.Utils.none<object>(), 0).Item2;
            var typeInfo = new TypeInfo(paramInfo, instance);
            var paramNode = TypeNodeBuilder.Create(paramInfo.Name ?? "??", paramInfo.ParameterType, typeInfo,
                new Dictionary<string, int>(), instance, null);
            paramNode.Init();
            methodNode.AddItem(paramNode);
            //methodNode.Items.Add(paramNode);
            counter += 1;
        }
        

        //setup the templated {{TAG}} nodes
        var methodInfoNode = (MethodInfoNode)this.Items[0];
        foreach (var envVar in envVars) {
            var node = methodInfoNode.FindChildNode(i => {
                if (i is SystemNode sn) {
                    var pathToRoot = sn.GetJsonPath();
                    return pathToRoot == envVar.JsonPath;
                }

                return false;
            });

            if (node is SystemNode sysNode) {
                sysNode.CreateEnvVariable(envVar.Tag, envVar.JsonPath);
            }
        }
        
        this.RaisePropertyChanged(nameof(this.Items));
    }

    public void StartRequest() {
    }
}