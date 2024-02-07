#region

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Tefin.ViewModels.Explorer;

#endregion

namespace Tefin.ViewModels;

public class NodeViewLocator : IDataTemplate {
    private static readonly Dictionary<Type, Type> Mapping = new();

    public Control Build(object? data) {
        if (data == null) {
            return new TextBlock { Text = "data cannot be null" };
        }

        var sourceType = data.GetType();
        if (Mapping.TryGetValue(sourceType, out var value)) {
            ((NodeBase)data).IsEditing = false;
            return (Control)Activator.CreateInstance(value)!;
        }

        var name = data.GetType().FullName!.Replace(".ViewModels", ".Views") + "View";
        var type = Type.GetType(name);

        if (type != null) {
            Mapping.Add(sourceType, type);
            ((NodeBase)data).IsEditing = false;
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is NodeBase;
}