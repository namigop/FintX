#region

using System.Reflection;

#endregion

namespace Tefin.ViewModels.Types;

public interface ITypeInfo {
    bool CanRead { get; }
    bool CanWrite { get; }
    FieldInfo? FieldInfo { get; }
    string Name { get; }
    PropertyInfo? PropertyInfo { get; }

    object? GetValue(object parent);

    void SetValue(object parentInstance, object? value);
}