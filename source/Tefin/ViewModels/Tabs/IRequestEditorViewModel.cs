#region

using System.Reflection;
using System.Threading;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IRequestEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public CancellationTokenSource? CtsReq { get; }
    public (bool, object?[]) GetParameters();

    public void Show(object?[] parameters);
    
}