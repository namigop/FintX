using System.Reflection;

namespace Tefin.ViewModels.Tabs;

public interface IResponseEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public Task Complete(Type responseType, Func<Task<object>> completeRead);

    public void Init();
    
    public Type? ResponseType { get; }

    public (bool, object?) GetResponse();

    void Show(object? resp, Type? responseType);
}