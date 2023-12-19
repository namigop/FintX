#region

using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;
using Tefin.ViewModels.Types.TypeNodeBuilders;

using TypeInfo = Tefin.ViewModels.Types.TypeInfo;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TreeRequestEditorViewModel : ViewModelBase, IRequestEditorViewModel {
    public CancellationTokenSource? CtsReq {
        get;
        private set;
    }

    public TreeRequestEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        //this.MethodParameterInstances = methodParameterInstances ?? new List<object?>();

        this.MethodInfo = methodInfo;
        this.MethodTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                        new GridLength(1, GridUnitType.Star)),
                    x => x.Items,
                    x => x.Items.Any(),
                    x => x.IsExpanded)
            }
        };


        this.Items.Add(new EmptyNode());
    }
    public HierarchicalTreeDataGridSource<IExplorerItem> MethodTree { get; }

    public ObservableCollection<IExplorerItem> Items { get; } = new();
    //public List<object?> MethodParameterInstances { get; }

    public MethodInfo MethodInfo {
        get;
    }

    public (bool, object?[]) GetParameters() {
        var mParams = this.Items[0].Items.Select(t => ((TypeBaseNode)t).Value).ToArray()!;
        var last = mParams.Last();
        this.CtsReq = null;
        if (last is CancellationToken token && token != CancellationToken.None) {
            this.CtsReq = new CancellationTokenSource();
            mParams[mParams.Length - 1] = this.CtsReq.Token;
        }
        
        return (mParams.Any(), mParams);
    }

    public void Show(object?[] parameters) {
        this.Items.Clear();
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        var methodNode = new MethodInfoNode(this.MethodInfo);
        this.Items.Add(methodNode);


        var counter = 0;
        foreach (var paramInfo in methodParams) {
            var instance = hasValues ? parameters[counter] : TypeBuilder.getDefault(paramInfo.ParameterType, true, Core.Utils.none<object>(), 0).Item2;
            var typeInfo = new TypeInfo(paramInfo, instance);
            var paramNode = TypeNodeBuilder.Create(paramInfo.Name ?? "??", paramInfo.ParameterType, typeInfo, new Dictionary<string, int>(), instance, null);
            paramNode.Init();
            methodNode.Items.Add(paramNode);
            counter += 1;
        }


        this.RaisePropertyChanged(nameof(this.Items));
    }

    public void StartRequest() {
        
    }

    public void EndRequest() {
        this.CtsReq = null;
    }
}