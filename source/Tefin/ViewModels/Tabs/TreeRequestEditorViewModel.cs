#region

using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeNodeBuilders;

using TypeInfo = Tefin.ViewModels.Types.TypeInfo;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TreeRequestEditorViewModel : ViewModelBase, IRequestEditorViewModel {
    private List<RequestVariable> _reqVars = [];
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

        /* check for nodes with env tag and update the value of
         *  the node with the env variable  */
        if (this.Items[0] is MethodInfoNode mn)
            mn.TryUpdateTemplatedChildNodes();
        
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

    


    public void Show(object?[] parameters, List<RequestVariable> reqVars, ProjectTypes.ClientGroup clientGroup) {
        if (this._reqVars.Count == 0)
            this._reqVars = reqVars;
        
        this.Items.Clear();
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        var methodNode = new MethodInfoNode(this.MethodInfo, clientGroup,  this._reqVars);
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
            counter += 1;
        }
        

        //----------------------------------
        //setup the templated {{TAG}} nodes
        //----------------------------------
        var methodInfoNode = (MethodInfoNode)this.Items[0];
        foreach (var reqVar in reqVars) {
            var node = methodInfoNode.FindChildNode(i => {
                if (i is SystemNode sn) {
                    var pathToRoot = sn.GetJsonPath();
                    return pathToRoot == reqVar.JsonPath;
                }

                return false;
            });

            var load = new LoadEnvVarsFeature();
            var envVar = load.FindEnvVar(methodNode.ClientGroup.Path, Current.Env, reqVar.Tag, this.Io);
            if (node is SystemNode sysNode) {
                sysNode.EnvVar.CreateEnvVariable(reqVar.Tag, reqVar.JsonPath, envVar?.CurrentValue);
            }
            
            if (node is TimestampNode tsNode) {
                tsNode.CreateEnvVariable(reqVar.Tag, reqVar.JsonPath);
            }
        }
        
        this.RaisePropertyChanged(nameof(this.Items));
    }

    public void StartRequest() {
    }
}