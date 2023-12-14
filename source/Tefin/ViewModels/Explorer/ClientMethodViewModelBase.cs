#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Explorer;

public abstract class ClientMethodViewModelBase : ViewModelBase {
    protected ClientMethodViewModelBase(MethodInfo mi) {
        this.MethodInfo = mi;
    }

    public abstract string ApiType { get; }
    public MethodInfo MethodInfo { get; }
}