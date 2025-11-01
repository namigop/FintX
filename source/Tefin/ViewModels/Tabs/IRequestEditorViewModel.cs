#region

using System.Reflection;
using System.Threading;

using Tefin.Core.Interop;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IRequestEditorViewModel {
    public CancellationTokenSource? CtsReq { get; }
    public MethodInfo MethodInfo { get; }

    void EndRequest();

    public (bool, object?[]) GetParameters();

    public void Show(object?[] methodParameterInstances, List<VarDefinition> parameters,
        ProjectTypes.ClientGroup clientGroup);

    void StartRequest();
}