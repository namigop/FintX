#region

using System.Reflection;

using ReactiveUI;

using Tefin.Core;

#endregion

namespace Tefin.ViewModels.Tabs;

public class JsonResponseEditorViewModel(MethodInfo methodInfo) : ViewModelBase, IResponseEditorViewModel {
    private string _json = "";

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public Type? ResponseType {
        get;
        private set;
    }

    public MethodInfo MethodInfo { get; } = methodInfo;

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        try {
            this.ResponseType = responseType;
            var resp = await completeRead();
            this.Json = Instance.indirectSerialize(responseType, resp);
        }
        catch (Exception ecx) {
            this.Io.Log.Error(ecx);
        }
    }

    public void Init() {
        this.Json = "";
    }

    public (bool, object?) GetResponse() {
        if (this.ResponseType != null) {
            var resp = Instance.indirectDeserialize(this.ResponseType, this.Json);
            return (true, resp);
        }

        return (false, null);
    }

    public void Show(object? resp, Type? responseType) {
        if (responseType != null) {
            this.ResponseType = responseType;
            this.Json = Instance.indirectSerialize(this.ResponseType, resp);
        }
    }
}