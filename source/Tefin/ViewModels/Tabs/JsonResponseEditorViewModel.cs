using System.Reflection;

using Avalonia.Controls;

using ReactiveUI;

using Tefin.Core;

namespace Tefin.ViewModels.Tabs;

public class JsonResponseEditorViewModel : ViewModelBase, IResponseEditorViewModel {
    private string _json;
    private Type? _responseType;
    public JsonResponseEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        this._json = "";
    }

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }
    
    public Type? ResponseType {
        get => this._responseType;
    }
    public MethodInfo MethodInfo { get; }

    public async Task Complete(Type responseType, Func<Task<object>> completeRead) {
        try {
            this._responseType = responseType;
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
        if (this._responseType != null) {
            var resp = Instance.indirectDeserialize(this._responseType, this.Json);
            return (true, resp);
        }

        return (false, null);
    }

    public void Show(object? resp, Type? responseType) {
        if (responseType != null) {
            this._responseType = responseType;
            this.Json = Instance.indirectSerialize(this._responseType, resp);
        }
    }
}