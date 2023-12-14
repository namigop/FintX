#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core.Execution;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryRespViewModel : ViewModelBase {
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private readonly TreeResponseEditorViewModel _treeRespEditor;
    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    public UnaryRespViewModel(MethodInfo methodInfo) {
        this._methodInfo = methodInfo;
        this.IsShowingResponseTreeEditor = true;

        this._treeRespEditor = new TreeResponseEditorViewModel(methodInfo);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this._treeRespEditor;
        this.SubscribeTo(vm => ((UnaryRespViewModel)vm).IsShowingResponseTreeEditor, this.OnIsShowingResponseTreeEditor);
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



    public void Show(bool ok, object response, Context context) {
        this.ResponseEditor.Complete(response.GetType(), () => Task.FromResult(response));
        // Dispatcher.UIThread.Post(() => {
        //     this.Items.Clear();
        //     var node = new ResponseNode(this._methodInfo.Name, response.GetType(), null, response, null); // TypeNodeBuilder.Create(this._methodInfo.Name, response);
        //     node.Init();
        //     this.Items.Add(node);
        // });
    }
}