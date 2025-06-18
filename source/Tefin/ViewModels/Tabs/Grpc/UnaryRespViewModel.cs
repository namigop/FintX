#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core.Execution;
using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class UnaryRespViewModel : ViewModelBase {
    private readonly JsonResponseEditorViewModel _jsonRespEditor;
    private readonly MethodInfo _methodInfo;
    private readonly ProjectTypes.ClientGroup _clientGroup;
    
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

    public TreeResponseEditorViewModel TreeResponseEditor { get; private set; }

    public bool IsShowingResponseTreeEditor {
        get => this._isShowingResponseTreeEditor;
        set => this.RaiseAndSetIfChanged(ref this._isShowingResponseTreeEditor, value);
    }

    public IResponseEditorViewModel ResponseEditor {
        get => this._responseEditor;
        set => this.RaiseAndSetIfChanged(ref this._responseEditor, value);
    }
    
    public AllVariableDefinitions EnvVariables { get; set; }
    public void Init(AllVariableDefinitions envVariables) {
        this.EnvVariables = envVariables;
        this.ResponseEditor.Init();
    }

    public void Show(object response) =>
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
        var (ok, resp) = this.TreeResponseEditor.GetResponse();
        this.ResponseEditor = this._jsonRespEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.EnvVariables, this.TreeResponseEditor.ResponseType);
        }
    }

    private void ShowAsTree() {
        var (ok, resp) = this._jsonRespEditor.GetResponse();
        this.ResponseEditor = this.TreeResponseEditor;
        if (ok) {
            this.ResponseEditor.Show(resp, this.EnvVariables, this._jsonRespEditor.ResponseType);
        }
    }
}