using Tefin.Core;

namespace Tefin.ViewModels.Types;

public class RequestVariable {
    public string Tag { get; init; } = "";
    public string TypeName { get; init; }= "";
    public string JsonPath { get; init; }= "";
    public RequestEnvVarScope Scope { get; init; }
    
}

 