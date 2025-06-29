#region

using System.Reflection;

using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IResponseEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public Type? ResponseType { get; }

    public Task Complete(Type responseType, Func<Task<object>> completeRead, List<VarDefinition> responseVariables);

    public (bool, object?) GetResponse();

    public void Init();

    void Show(object? resp, List<VarDefinition> variable, Type? responseType);
}