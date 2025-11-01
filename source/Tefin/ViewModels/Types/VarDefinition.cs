using Tefin.Core;

namespace Tefin.ViewModels.Types;

public class VarDefinition {
    public string JsonPath { get; init; } = "";
    public RequestEnvVarScope Scope { get; init; }
    public string Tag { get; init; } = "";
    public string TypeName { get; init; } = "";
}