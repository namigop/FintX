using System.Linq;
using System.Reflection;

using ReactiveUI;

using Tefin.Grpc.Dynamic;
using Tefin.Utils;

namespace Tefin.ViewModels.Tabs;

public class JsonRequestEditorViewModel : ViewModelBase, IRequestEditorViewModel {
    private string _json;
    public JsonRequestEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        this._json = "";
    }

    public MethodInfo MethodInfo {
        get;
    }

    public (bool, object?[]) GetParameters() {
        var ret = DynamicTypes.fromJsonRequest(this.MethodInfo, this.Json);
        if (ret.IsOk & ret.ResultValue != null) {
            var val = ret.ResultValue;
            return val!.GetType().GetProperties()
                .Select(prop => prop.GetValue(val))
                .ToArray()
                .Then(v => (true, v));
        }

        return (false, Array.Empty<object>());
    }


    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public void Show(object?[] parameters) {
        var json = DynamicTypes.toJsonRequest(SerParam.Create(this.MethodInfo, parameters));
        if (json.IsOk)
            this.Json = json.ResultValue;
    }
}