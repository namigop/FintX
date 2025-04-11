using Tefin.Core;

namespace Tefin.ViewModels.Types;

public class RequestVariable {
    public string Tag { get; set; }
    public string TypeName { get; set; }
    public string JsonPath { get; set; }
    
    public RequestEnvVarScope Scope { get; set; }
    
}

 