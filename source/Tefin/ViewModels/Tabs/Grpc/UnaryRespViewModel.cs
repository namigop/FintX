#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Core.Interop;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryRespViewModel : ViewModelBase {
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly TreeResponseEditorViewModel _treeRespEditor;
    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    public UnaryRespViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup clientGroup) {
        this._methodInfo = methodInfo;
        this._clientGroup = clientGroup;
        this.IsShowingResponseTreeEditor = true;

        this._treeRespEditor = new TreeResponseEditorViewModel(methodInfo);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this._treeRespEditor;
        this.SubscribeTo(vm => ((UnaryRespViewModel)vm).IsShowingResponseTreeEditor,
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

    public void Init() => this.ResponseEditor.Init();

    public void Show(bool ok, object response, Context context) =>
        this.ResponseEditor.Complete(response.GetType(), () => Task.FromResult(response));

    private void OnIsShowingResponseTreeEditor(ViewModelBase obj) {
        try {
            var vm = (UnaryRespViewModel)obj;
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
            this.ResponseEditor.Show(resp, this._treeRespEditor.ResponseType);
        }
    }

    private void ShowAsTree() {
        var (ok, resp) = this._jsonRespEditor.GetResponse();
        this.ResponseEditor = this._treeRespEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this._jsonRespEditor.ResponseType);
        }
    }
}