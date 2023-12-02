#region

using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Microsoft.FSharp.Core;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeNodeBuilders;

using TypeInfo = Tefin.ViewModels.Types.TypeInfo;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryReqViewModel : ViewModelBase {
    private readonly bool _generateFullTree;

    public UnaryReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object>? methodParameterInstances = null) {
        this.MethodParameterInstances = methodParameterInstances ?? new List<object>();
        this._generateFullTree = generateFullTree;
        this.MethodInfo = methodInfo;
        this.MethodTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };

        this.Items.Add(new EmptyNode());
    }

    public ObservableCollection<IExplorerItem> Items { get; } = new();
    public MethodInfo MethodInfo { get; }
    public List<object> MethodParameterInstances { get; }

    public HierarchicalTreeDataGridSource<IExplorerItem> MethodTree { get; }

    public object[] GetMethodParameters() {
        return this.Items[0].Items.Select(t => ((TypeBaseNode)t).Value).ToArray();
    }

    public void Init() {
        if (this.Items.FirstOrDefault() is not EmptyNode) return;

        this.Items.Clear();
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = this.MethodParameterInstances.Count == methodParams.Length;

        var methodNode = new MethodInfoNode(this.MethodInfo);
        this.Items.Add(methodNode);

        if (this._generateFullTree) {
            var counter = 0;
            List<object> mParams = new();
            foreach (var paramInfo in methodParams) {
                var instance = hasValues ? this.MethodParameterInstances[counter] : TypeBuilder.getDefault(paramInfo.ParameterType, true, FSharpOption<object>.None, 0).Item2;
                TypeInfo? p = new(paramInfo, instance);
                var paramNode = TypeNodeBuilder.Create(paramInfo.Name, paramInfo.ParameterType, p, new Dictionary<string, int>(), instance, null);
                paramNode.Init();
                methodNode.Items.Add(paramNode);
                counter += 1;

                if (!hasValues)
                    this.MethodParameterInstances.Add(instance);
            }
        }

        this.RaisePropertyChanged(nameof(this.Items));
    }
}