#region

using System.Reflection;

using Grpc.Core;

using ReactiveUI;

using Tefin.Core.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public abstract class StandardResponseViewModel : ViewModelBase {
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private readonly TreeResponseEditorViewModel _treeRespEditor;
    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    protected StandardResponseViewModel(MethodInfo methodInfo) {
        this._methodInfo = methodInfo;
        this.IsShowingResponseTreeEditor = true;

        this._treeRespEditor = new TreeResponseEditorViewModel(methodInfo);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this._treeRespEditor;
        this.SubscribeTo(vm => ((StandardResponseViewModel)vm).IsShowingResponseTreeEditor, this.OnIsShowingResponseTreeEditor);
    }

    public IResponseEditorViewModel ResponseEditor {
        get => this._responseEditor;
        set => this.RaiseAndSetIfChanged(ref this._responseEditor, value);
    }
    public bool IsShowingResponseTreeEditor {
        get => this._isShowingResponseTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._isShowingResponseTreeEditor, value);
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
    private void ShowAsJson() {
        var (ok, resp) = this._treeRespEditor.GetResponse();
        this.ResponseEditor = this._jsonRespEditor;
        if (ok)
            this.ResponseEditor.Show(resp, this._treeRespEditor.ResponseType);
    }

    private void ShowAsTree() {
        var (ok, resp) = this._jsonRespEditor.GetResponse();
        this.ResponseEditor = this._treeRespEditor;
        if (ok)
            this.ResponseEditor.Show(resp, this._jsonRespEditor.ResponseType);
    }


    public void Init() {
        this.ResponseEditor.Init();
    }

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        var response = await completeRead();
        responseType = response?.GetType() ?? responseType;
        await this.ResponseEditor.Complete(responseType, () => Task.FromResult(response!));
    }

    public abstract void Show(bool ok, object response, Context context);

    public class GrpcStandardResponse {
        public required Metadata Headers { get; set; }
        public required Status Status { get; set; }
        public required Metadata Trailers { get; set; }
    }
}