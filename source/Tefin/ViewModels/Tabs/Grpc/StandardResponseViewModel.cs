#region

using System.Reflection;
using System.Threading.Tasks;

using Grpc.Core;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public abstract class StandardResponseViewModel : ViewModelBase {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    protected StandardResponseViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup cg) {
        this._methodInfo = methodInfo;
        this._clientGroup = cg;
        this.IsShowingResponseTreeEditor = true;

        this.TreeResponseEditor = new TreeResponseEditorViewModel(methodInfo, cg);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this.TreeResponseEditor;
        this.SubscribeTo(vm => ((StandardResponseViewModel)vm).IsShowingResponseTreeEditor,
            this.OnIsShowingResponseTreeEditor);
    }

    public bool IsShowingResponseTreeEditor {
        get => this._isShowingResponseTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._isShowingResponseTreeEditor, value);
    }

    public IResponseEditorViewModel ResponseEditor {
        get => this._responseEditor;
        set => this.RaiseAndSetIfChanged(ref this._responseEditor, value);
    }

    public List<VarDefinition> ResponseVariables { get; protected set; }

    public TreeResponseEditorViewModel TreeResponseEditor { get; }

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        var response = await completeRead();
        responseType = response?.GetType() ?? responseType;
        await this.ResponseEditor.Complete(responseType, () => Task.FromResult(response!), this.ResponseVariables);
    }

    public abstract void Init(AllVariableDefinitions envVariables);
    
    public void ShowFailure(Exception? error) {
        var failure = new GrpcCallFailure();

        switch (error) {
            case RpcException rpc:
                failure.Status = rpc.StatusCode.ToString();
                failure.Error = rpc.Status.Detail;
                break;
            case not null:
                failure.Error = error.Message;
                break;
        }

        var task = this.ResponseEditor.Complete(typeof(GrpcCallFailure), () => Task.FromResult<object>(failure),
            this.ResponseVariables);
        this.ObserveComplete(task);
    }

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

    public abstract void Show(bool ok, object response, Context context);

    private void ShowAsJson() {
        var (ok, resp) = this.TreeResponseEditor.GetResponse();
        this.ResponseEditor = this._jsonRespEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.ResponseVariables, this.TreeResponseEditor.ResponseType);
        }
    }

    private void ShowAsTree() {
        var (ok, resp) = this._jsonRespEditor.GetResponse();
        this.ResponseEditor = this.TreeResponseEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.ResponseVariables, this._jsonRespEditor.ResponseType);
        }
    }

    public class GrpcStandardResponse {
        public Metadata Headers { get; set; } = new();
        public Status Status { get; set; }
        public Metadata Trailers { get; set; } = new();
    }

    /// <summary>
    /// A failed call, flattened for display.
    /// </summary>
    public class GrpcCallFailure {
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";
    }
}