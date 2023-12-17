#region

using System.Linq;
using System.Reflection;
using System.Threading;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.Grpc.Dynamic;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Tabs;

public class JsonRequestEditorViewModel : ViewModelBase, IRequestEditorViewModel {
    private string _json;
    public JsonRequestEditorViewModel(MethodInfo methodInfo) {
        this.MethodInfo = methodInfo;
        this._json = "";
    }

    public CancellationTokenSource? CtsReq {
        get;
        private set;
    }

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public MethodInfo MethodInfo {
        get;
    }

    public (bool, object?[]) GetParameters() {
        var ret = DynamicTypes.fromJsonRequest(this.MethodInfo, this.Json);
        if (ret.IsOk & ret.ResultValue != null) {
            var val = ret.ResultValue;
            var mParams = val!.GetType().GetProperties()
                .Select(prop => prop.GetValue(val))
                .ToArray();
            var last = mParams.Last();
            this.CtsReq = null;
            if (last is CancellationToken token && token != CancellationToken.None) {
                this.CtsReq = new CancellationTokenSource();
                mParams[mParams.Length - 1] = this.CtsReq.Token;
            }

            return (true, mParams);

        }

        return (false, Array.Empty<object>());
    }

    public void Show(object?[] parameters) {
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        if (!hasValues) {
            parameters = methodParams.Select(paramInfo => {
                var (ok, inst) = TypeBuilder.getDefault(paramInfo.ParameterType, true, Core.Utils.none<object>(), 0);
                return inst;
            }).ToArray();
        }

        var json = DynamicTypes.toJsonRequest(SerParam.Create(this.MethodInfo, parameters));
        if (json.IsOk)
            this.Json = json.ResultValue;
    }
}