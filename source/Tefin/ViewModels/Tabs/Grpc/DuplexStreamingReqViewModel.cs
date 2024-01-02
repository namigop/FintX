#region

using System.Reflection;
using System.Windows.Input;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Execution;

using static Tefin.Core.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public class DuplexStreamingReqViewModel : UnaryReqViewModel {
    private readonly ListJsonEditorViewModel _clientStreamJsonEditor;
    private readonly ListTreeEditorViewModel _clientStreamTreeEditor;
    private readonly Type _listType;
    private readonly Type _requestItemType;
    private DuplexStreamingCallResponse? _callResponse;
    private bool _canWrite;

    private IListEditorViewModel _clientStreamEditor;
    private bool _isShowingClientStreamTree;
    public DuplexStreamingReqViewModel(MethodInfo methodInfo, bool generateFullTree, List<object?>? methodParameterInstances = null)
        : base(methodInfo, generateFullTree, methodParameterInstances) {
        this.WriteCommand = this.CreateCommand(this.OnWrite);
        this.EndWriteCommand = this.CreateCommand(this.OnEndWrite);
        this._callResponse = DuplexStreamingCallResponse.Empty();
        var args = methodInfo.ReturnType.GetGenericArguments();
        this._requestItemType = args[0];
        var listType = typeof(List<>);
        this._listType = listType.MakeGenericType(this._requestItemType);

        this._clientStreamTreeEditor = new ListTreeEditorViewModel("Request Stream", this._listType);
        this._clientStreamJsonEditor = new ListJsonEditorViewModel("Request Stream", this._listType);
        this._isShowingClientStreamTree = true;
        this._clientStreamEditor = this._clientStreamTreeEditor;

        this.SubscribeTo(vm => ((ClientStreamingReqViewModel)vm).IsShowingClientStreamTree, this.OnIsShowingClientStreamTreeChanged);
    }

    public DuplexStreamingCallResponse? CallResponse {
        get => this._callResponse;
        private set => this.RaiseAndSetIfChanged(ref this._callResponse, value);
    }

    public ICommand EndWriteCommand { get; }

    public ICommand WriteCommand { get; }
    public bool CanWrite {
        get => this._canWrite;
        private set => this.RaiseAndSetIfChanged(ref this._canWrite, value);
    }
    public bool IsShowingClientStreamTree {
        get => this._isShowingClientStreamTree;
        set => this.RaiseAndSetIfChanged(ref this._isShowingClientStreamTree, value);
    }
    public IListEditorViewModel ClientStreamEditor {
        get => this._clientStreamEditor;
        private set => this.RaiseAndSetIfChanged(ref this._clientStreamEditor, value);
    }

    public void SetupDuplexStream(DuplexStreamingCallResponse response) {
        this._callResponse = response;
        var stream = Activator.CreateInstance(this._listType)!;
        var (ok, reqInstance) = TypeBuilder.getDefault(this._requestItemType, true, none<object>(), 0);
        if (ok) {
            var add = this._listType.GetMethod("Add");
            add!.Invoke(stream, new[] {
                reqInstance
            });
        }
        else
            this.Io.Log.Error($"Unable to create an instance for {this._requestItemType}");

        this._clientStreamEditor.Show(stream!);
        this.CanWrite = true;
    }
    private void OnIsShowingClientStreamTreeChanged(ViewModelBase obj) {
        var vm = (DuplexStreamingReqViewModel)obj;
        if (vm._isShowingClientStreamTree) {
            this.ShowAsTree();
        }
        else {
            this.ShowAsJson();
        }
    }

    private void ShowAsJson() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this._clientStreamJsonEditor;
        if (ok)
            this.ClientStreamEditor.Show(list);
    }

    private void ShowAsTree() {
        var (ok, list) = this._clientStreamEditor.GetList();
        this.ClientStreamEditor = this._clientStreamTreeEditor;
        if (ok)
            this.ClientStreamEditor.Show(list);
    }

    private async Task OnEndWrite() {
        try {
            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;
            if (this.CallResponse != null)
                this.CallResponse = await writer.CompleteWrite(this.CallResponse);
        }
        finally {
            this.CanWrite = false;
            //this.ClientStreamEditor.Clear();
            this.IsBusy = false;
        }
    }

    public override async Task ImportRequest() {
        await GrpcUiUtils.ImportRequest(this.RequestEditor, this.ClientStreamEditor, this._listType, this.MethodInfo, this.Io);
    }
    public string GetRequestContent() {
        var (ok, mParams) = this.GetMethodParameters();
        if (ok) {
            var (isValid, reqStream) = this.ClientStreamEditor.GetList();
            if (isValid) {
                var feature = new ExportFeature(this.MethodInfo, mParams, reqStream);
                var exportReqJson = feature.Export();
                if (exportReqJson.IsOk) {
                    return exportReqJson.ResultValue;
                }
            }
        }

        return "";
    }
    public override async Task ExportRequest() {
        var (ok, mParams) = this.GetMethodParameters();
        if (ok) {
            var (isValid, reqStream) = this.ClientStreamEditor.GetList();
            if (!isValid) {
                this.Io.Log.Warn("Request stream is invalid. Content will not be saved to the request file");
            }

            await GrpcUiUtils.ExportRequest(mParams, reqStream, this.MethodInfo, this.Io);
        }
    }
    private async Task OnWrite() {
        try {
            if (this.CallResponse == null) {
                this.Io.Log.Warn("Unable to write to the request stream");
                return;
            }

            var resp = this.CallResponse;
            var writer = new WriteDuplexStreamFeature();
            this.IsBusy = true;

            foreach (var i in this.ClientStreamEditor.GetListItems())
                await writer.Write(resp, i);
        }
        catch (Exception exc) {
            this.Io.Log.Error(exc);
        }
        finally {
            this.IsBusy = false;
        }
    }
}