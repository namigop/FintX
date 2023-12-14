#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IResponseEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public Type? ResponseType { get; }

    public Task Complete(Type responseType, Func<Task<object>> completeRead);

    public void Init();

    public (bool, object?) GetResponse();

    void Show(object? resp, Type? responseType);
}