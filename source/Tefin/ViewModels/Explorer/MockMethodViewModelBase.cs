using System.Reflection;

namespace Tefin.ViewModels.Explorer;

public abstract class MockMethodViewModelBase(MethodInfo mi) : ViewModelBase {
    public abstract string ApiType { get; }

    public abstract bool IsLoaded { get; }
    public MethodInfo MethodInfo { get; } = mi;

    public abstract string GetScriptContent();

    public abstract void ImportScript(string requestFile);
}