#region

using System.Reflection;
using System.Threading;

using Newtonsoft.Json.Linq;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc.Dynamic;

#endregion

namespace Tefin.ViewModels.Tabs;

public class JsonRequestEditorViewModel(MethodInfo methodInfo) : ViewModelBase, IRequestEditorViewModel {
    private string _json = "";
    private RequestEnvVar[] _envVars = [];

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
         
        if (this._envVars.Length > 0) {
            var projEnv = VarsStructure.getVars(this.Io, Current.ProjectPath);
            var curEnv = projEnv.Variables.FirstOrDefault(t => t.Item2.Name == Current.Env);
            if (curEnv != null) {
                var jsonObject = JObject.Parse(this.Json);
                foreach (var e in this._envVars) {
                    var token = jsonObject.SelectToken(e.JsonPath);
                    var newValue = GetFromEnvFile(e.Tag, curEnv.Item2);
                    if (newValue != null)
                        token?.Replace(newValue); // Replace the value
                }

                this.Json = jsonObject.ToString();
            }

        }
        
        
        var ret = DynamicTypes.fromJsonRequest(this.MethodInfo, this.Json);
        if (ret is { IsOk: true, ResultValue: not null }) {
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

        string? GetFromEnvFile(string name, EnvConfigData data) {
           var env = data.Variables.FirstOrDefault(t => t.Name == name);
           return env?.CurrentValue;
        }

    }

    public void Show(object?[] parameters, RequestEnvVar[] envVars) {
        this._envVars = envVars;
        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        if (!hasValues) {
            parameters = methodParams.Select(paramInfo => {
                var (ok, inst) = TypeBuilder.getDefault(paramInfo.ParameterType, true, Core.Utils.none<object>(), 0);
                return inst;
            }).ToArray();
        }

      
        var json = DynamicTypes.toJsonRequest(SerParam.Create(this.MethodInfo, parameters));
        if (json.IsOk) {
            this.Json = json.ResultValue;
        }
    }

    public void StartRequest() {
    }
}