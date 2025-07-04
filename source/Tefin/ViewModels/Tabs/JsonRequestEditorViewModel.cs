#region

using System.Reflection;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ReactiveUI;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Grpc.Dynamic;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public class JsonRequestEditorViewModel(MethodInfo methodInfo) : ViewModelBase, IRequestEditorViewModel {
    private static string[] DisplayTypes = SystemType.getTypesForDisplay();
    private static Type[] ActualTypes = SystemType.getTypes().ToArray();
    
    private string _json = "";
    private List<VarDefinition> _envVars = [];

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
         
        if (this._envVars.Count > 0) {
            //Update the JSON with the env var values from .fxv
            var curEnv = VarsStructure.getVarsFromFile(this.Io, Current.EnvFilePath); 
            if (curEnv != null) {
                var jsonObject = JObject.Parse(this.Json);
                foreach (var e in this._envVars) {
                    var token = jsonObject.SelectToken(e.JsonPath);
                    var newValue =  curEnv.Variables.FirstOrDefault(t => t.Name == e.Tag)?.CurrentValue;
                    var type = ActualTypes.First(t => t.FullName == e.TypeName);
                    var typedValue = TypeHelper.indirectCast(newValue, type);
                    var writer = new JTokenWriter();
                    JsonSerializer.Create().Serialize(writer, typedValue);
  
                    writer.Close();
                    if (newValue != null)
                        token?.Replace(writer.Token!); // Replace the value
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
    }

    public void Show(object?[] parameters, List<VarDefinition> envVars, ProjectTypes.ClientGroup clientGroup) {
        if (this._envVars.Count == 0)
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