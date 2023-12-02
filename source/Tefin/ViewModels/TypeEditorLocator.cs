#region

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Tefin.ViewModels.Types.TypeEditors;

#endregion

namespace Tefin.ViewModels;

public class TypeEditorLocator : IDataTemplate {
    private static readonly Dictionary<Type, Type> mapping = new();

    public Control Build(object data) {
        var sourceType = data.GetType();
        if (mapping.TryGetValue(sourceType, out var value))
            return (Control)Activator.CreateInstance(value)!;

        var fullName = sourceType.FullName!.Replace("ViewModels", "Views") + "View";

        var type = Type.GetType(fullName);

        if (type != null) {
            mapping.Add(sourceType, type);
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + fullName };
    }

    public bool Match(object data) {
        return data is ITypeEditor;
    }
}