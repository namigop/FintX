#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Tabs;

public interface IRequestEditorViewModel {
    public MethodInfo MethodInfo { get; }

    public (bool, object?[]) GetParameters();

    public void Show(object?[] parameters);
}