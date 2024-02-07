#region

using System.Collections.ObjectModel;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public class TreeResponseEditorViewModel : ViewModelBase, IResponseEditorViewModel {
    public TreeResponseEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        this.ResponseTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate",
                        "CellEditTemplate", //edittemplate
                        new GridLength(1, GridUnitType.Star)),
                    x => x.Items, //
                    x => x.Items.Any(), //
                    x => x.IsExpanded) //
            }
        };
    }

    public ObservableCollection<IExplorerItem> Items { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> ResponseTree { get; }
    public MethodInfo MethodInfo { get; }

    public Type? ResponseType {
        get;
        private set;
    }

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        this.Items.Clear();
        try {
            this.ResponseType = responseType;
            var resp = await completeRead();
            var node = new ResponseNode(this.MethodInfo.Name, responseType, null, resp, null);
            node.Init();
            this.Items.Add(node);
        }
        catch (Exception ecx) {
            this.Io.Log.Error(ecx);
        }
    }

    public (bool, object?) GetResponse() {
        if (this.ResponseType == null) {
            return (false, null);
        }

        if (this.Items.Any()) {
            var node = (TypeBaseNode)this.Items[0];
            return (node.Value != null, node.Value);
        }

        return (false, null);
    }

    public void Init() => this.Items.Clear();

    public void Show(object? resp, Type? responseType) {
        this.Items.Clear();
        if (responseType == null) {
            return;
        }

        try {
            this.ResponseType = responseType;
            var node = new ResponseNode(this.MethodInfo.Name, this.ResponseType, null, resp, null);
            node.Init();
            this.Items.Add(node);
        }
        catch (Exception ecx) {
            this.Io.Log.Error(ecx);
        }
    }
}