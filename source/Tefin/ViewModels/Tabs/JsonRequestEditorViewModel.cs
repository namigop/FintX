#region

using System.Reflection;
using System.Threading;

using ReactiveUI;

using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Dynamic;

#endregion

namespace Tefin.ViewModels.Tabs;

public class JsonRequestEditorViewModel(MethodInfo methodInfo) : ViewModelBase, IRequestEditorViewModel {
    private string _json = "";

    public string Json {
        get => this._json;
        set => this.RaiseAndSetIfChanged(ref this._json, value);
    }

    public CancellationTokenSource? CtsReq {
        get;
        private set;
    }

    public MethodInfo MethodInfo {
        get;
    } = methodInfo;

    public void EndRequest() => this.CtsReq = null;

    public (bool, object?[]) GetParameters() {
        //howtofahndle when this.Json is templated
        var ret = DynamicTypes.fromJsonRequest(this.MethodInfo, this.Json);
        if (ret.IsOk & (ret.ResultValue != null)) {
            var val = ret.ResultValue;
            var mParams = val!.GetType().GetProperties()
                .Select(prop => prop.GetValue(val))
                .ToArray();
            var last = mParams.Last();
            
            if (last is CancellationToken) {
                if (CtsReq == null) {
                    this.CtsReq = new CancellationTokenSource();
                }
                //replace the CancellationToken with one that we can cancel
                mParams[^1] = this.CtsReq.Token;
            }

            return (true, mParams);
        }

        return (false, []);
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

        // var feature = new ExportFeature(this.MethodInfo, parameters);
        // var exportReqJson = feature.Export();
        // if (exportReqJson.IsOk) {
        //     this.Json =  exportReqJson.ResultValue;
        // }

        var json = DynamicTypes.toJsonRequest(SerParam.Create(this.MethodInfo, parameters));
        if (json.IsOk) {
            this.Json = json.ResultValue;
        }
    }

    public void StartRequest() {
    }
}