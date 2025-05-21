#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Explorer;

public abstract class ClientMethodViewModelBase(MethodInfo mi) : ViewModelBase {
    public abstract string ApiType { get; }
    public MethodInfo MethodInfo { get; } = mi;
    
    public abstract bool IsLoaded { get; }

    public abstract string GetRequestContent();

    public abstract void ImportRequestFile(string requestFile);
}