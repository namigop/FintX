#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryRespViewModel : ViewModelBase {
    private readonly ProjectTypes.ClientGroup _clientGroup;
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;

    private bool _isShowingResponseTreeEditor;
    private IResponseEditorViewModel _responseEditor;

    public UnaryRespViewModel(MethodInfo methodInfo, ProjectTypes.ClientGroup clientGroup) {
        this._methodInfo = methodInfo;
        this._clientGroup = clientGroup;
        this.IsShowingResponseTreeEditor = true;

        this.TreeResponseEditor = new TreeResponseEditorViewModel(methodInfo, clientGroup);
        this._jsonRespEditor = new JsonResponseEditorViewModel(methodInfo);
        this._responseEditor = this.TreeResponseEditor;
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

    public List<VarDefinition> ResponseVariables { get; set; }

    public TreeResponseEditorViewModel TreeResponseEditor { get; }

    public void Init(AllVariableDefinitions envVariables) {
        this.ResponseVariables = envVariables.ResponseVariables;
        this.ResponseEditor.Init();
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

    public void Show(object response) =>
        this.ResponseEditor.Complete(response.GetType(), () => Task.FromResult(response), this.ResponseVariables);

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
}