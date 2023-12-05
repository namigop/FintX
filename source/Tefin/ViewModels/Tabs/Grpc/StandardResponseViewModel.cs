using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;

using DynamicData;

using Grpc.Core;

using Tefin.Core.Execution;
using Tefin.ViewModels.Explorer;
using Tefin.ViewModels.Types;

namespace Tefin.ViewModels.Tabs.Grpc;

public class StandardResponseViewModel : ViewModelBase {
    private readonly MethodInfo _methodInfo;

    public StandardResponseViewModel(MethodInfo methodInfo) {
        this._methodInfo = methodInfo;
        this.ResponseTree = new HierarchicalTreeDataGridSource<IExplorerItem>(this.Items) {
            Columns = {
                new HierarchicalExpanderColumn<IExplorerItem>(new NodeTemplateColumn<IExplorerItem>("", "CellTemplate", "CellEditTemplate", //edittemplate
                    new GridLength(1, GridUnitType.Star)), x => x.Items, x => x.Items.Any(), x => x.IsExpanded)
            }
        };
        
        //this.Items.Add(new EmptyNode());
    }

    public ObservableCollection<IExplorerItem> Items { get; } = new();
    public HierarchicalTreeDataGridSource<IExplorerItem> ResponseTree { get; }

    public void Init() {
        this.Items.Clear();
    }

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        this.Items.Clear();
        try {
            var resp = await completeRead();
            var node = new ResponseNode(this._methodInfo.Name, responseType, null, resp, null);
            node.Init();
            this.Items.Add(node);
        }
        catch (Exception ecx) {
            Io.Log.Error(ecx);
        }
    }

    public virtual void Show(bool ok, object response, Context context) {
        if (ok) {
            var model = new GrpcStandardResponse();
            var node = new ResponseNode(this._methodInfo.Name, typeof(GrpcStandardResponse), null, model, null);
            node.Init();
            this.Items.Clear();
            this.Items.Add(node);
        }
    }

    public class GrpcStandardResponse {
        public Metadata Headers { get; set; }
        public Status Status { get; set; }
        public Metadata Trailers { get; set; }
    }
}