#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IResponseEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public Type? ResponseType { get; }

    public Task Complete(Type responseType, Func<Task<object>> completeRead);

    public (bool, object?) GetResponse();

    public void Init();

    void Show(object? resp, Type? responseType);
}