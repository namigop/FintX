#region

using System.Reflection;

using Grpc.Core;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public abstract class StandardResponseViewModel : ViewModelBase {
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly TreeResponseEditorViewModel _treeRespEditor;
    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    protected StandardResponseViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        this._methodInfo = methodInfo;
        this._clientGroup = cg;
        this.IsShowingResponseTreeEditor = true;

        this._treeRespEditor = new TreeResponseEditorViewModel(methodInfo, cg);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this._treeRespEditor;
        this.SubscribeTo(vm => ((StandardResponseViewModel)vm).IsShowingResponseTreeEditor,
            this.OnIsShowingResponseTreeEditor);
    }

    public bool IsShowingResponseTreeEditor {
        get => this._isShowingResponseTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._isShowingResponseTreeEditor, value);
    }

    public TreeResponseEditorViewModel TreeResponseEditor => this._treeRespEditor;
    public IResponseEditorViewModel ResponseEditor {
        get => this._responseEditor;
        set => this.RaiseAndSetIfChanged(ref this._responseEditor, value);
    }
    public List<RequestVariable> ResponseVariables { get; protected set; }
    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        var response = await completeRead();
        responseType = response?.GetType() ?? responseType;
        await this.ResponseEditor.Complete(responseType, () => Task.FromResult(response!));
    }

    public abstract void Init(AllVariableDefinitions envVariables);

    public abstract void Show(bool ok, object response, Context context);

    private void OnIsShowingResponseTreeEditor(ViewModelBase obj) {
        try {
            var vm = (StandardResponseViewModel)obj;
            if (vm._isShowingResponseTreeEditor) {
                this.ShowAsTree();
            }
            else {
                this.ShowAsJson();
            }
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
    }

    private void ShowAsJson() {
        var (ok, resp) = this._treeRespEditor.GetResponse();
        this.ResponseEditor = this._jsonRespEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.ResponseVariables, this._treeRespEditor.ResponseType);
        }
    }

    private void ShowAsTree() {
        var (ok, resp) = this._jsonRespEditor.GetResponse();
        this.ResponseEditor = this._treeRespEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.ResponseVariables, this._jsonRespEditor.ResponseType);
        }
    }

    public class GrpcStandardResponse {
        public Metadata Headers { get; set; } = new();
        public Status Status { get; set; }
        public Metadata Trailers { get; set; } = new();
    }
}