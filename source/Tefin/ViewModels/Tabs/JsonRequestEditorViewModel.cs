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
    private static readonly Type[] ActualTypes = SystemType.getTypes().ToArray();
    private List<VarDefinition> _envVars = [];

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

    public string? LastParametersError {
        get;
        private set;
    }

    public void EndRequest() => this.CtsReq = null;

    public (bool, object?[]) GetParameters() {
        if (this._envVars.Count > 0 && !this.TryApplyEnvVars()) {
            return (false, []);
        }

        var ret = DynamicTypes.fromJsonRequest(this.MethodInfo, this.Json);
        if (ret is { IsOk: true, ResultValue: not null }) {
            var val = ret.ResultValue;
            var mParams = val!.GetType().GetProperties()
                .Select(prop => prop.GetValue(val))
                .ToArray();
            var last = mParams.Last();

            if (last is CancellationToken) {
                if (this.CtsReq == null) {
                    this.CtsReq = new CancellationTokenSource();
                }

                //replace the CancellationToken with one that we can cancel
                mParams[^1] = this.CtsReq.Token;
            }

            this.LastParametersError = null;
            return (true, mParams);
        }

        //the request could not be built. Report why instead of silently returning an
        //empty parameter set, the caller must not substitute defaults for a request
        //that the user typed.
        if (ret.IsError) {
            this.LastParametersError = ret.ErrorValue?.Message ?? "the parameters could not be built";
            this.Io.Log.Error(ret.ErrorValue);
        }
        else {
            //jsonDeserialize logs the parser exception and returns null, so the cause is in
            //the log rather than on this result. A literal null document lands here too.
            this.LastParametersError = "the request JSON did not produce a request";
        }

        return (false, []);
    }

    /// <summary>
    /// Replaces the values in the JSON with the ones from the current env file. Returns false
    /// when the JSON cannot be read or a variable cannot be applied, having recorded why in
    /// <see cref="LastParametersError"/>.
    /// </summary>
    private bool TryApplyEnvVars() {
        try {
            var curEnv = VarsStructure.getVarsFromFile(this.Io, Current.EnvFilePath);
            if (curEnv == null) {
                return true;
            }

            var jsonObject = JObject.Parse(this.Json);
            foreach (var e in this._envVars) {
                var token = jsonObject.SelectToken(e.JsonPath);
                var newValue = curEnv.Variables.FirstOrDefault(t => t.Name == e.Tag)?.CurrentValue;
                var type = ActualTypes.First(t => t.FullName == e.TypeName);
                var typedValue = TypeHelper.indirectCast(newValue, type);
                var writer = new JTokenWriter();
                JsonSerializer.Create().Serialize(writer, typedValue);

                writer.Close();
                if (newValue != null) {
                    token?.Replace(writer.Token!); // Replace the value
                }
            }

            this.Json = jsonObject.ToString();
            return true;
        }
        catch (Exception exc) {
            //this used to throw out of Send, which then showed the user nothing at all
            this.LastParametersError = $"the environment variables could not be applied ({exc.Message})";
            this.Io.Log.Error(exc);
            return false;
        }
    }

    public void Show(object?[] parameters, List<VarDefinition> envVars, ProjectTypes.ClientGroup clientGroup) {
        if (this._envVars.Count == 0) {
            this._envVars = envVars;
        }

        var methodParams = this.MethodInfo.GetParameters();
        var hasValues = parameters.Length == methodParams.Length;

        if (!hasValues) {
            parameters = methodParams.Select(paramInfo => {
                var (ok, inst) = TypeBuilder.getDefault(paramInfo.Name, paramInfo.ParameterType, true, Core.Utils.none<object>(), 0);
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