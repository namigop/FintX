using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Tefin.ViewModels.Explorer;

namespace Tefin.ViewModels;

public class NodeActionViewLocator : IDataTemplate {
    private static readonly Dictionary<Type, Type> Mapping = new();

    public Control Build(object? data) {
        if (data == null)
            return new Border() { Width = 0, Height = 0 };
        
        var sourceType = data.GetType();
        if (Mapping.TryGetValue(sourceType, out var value)) {
            ((NodeBase)data).IsEditing = false;
            return (Control)Activator.CreateInstance(value)!;
        }

        var name = data.GetType().FullName!.Replace(".ViewModels", ".Views") + "ActionView";
        var type = Type.GetType(name);

        if (type != null) {
            Mapping.Add(sourceType, type);
            ((NodeBase)data).IsEditing = false;
            return (Control)Activator.CreateInstance(type)!;
        }

        return new Border() { Width = 0, Height = 0 };
    }

    public bool Match(object? data) {
        return data is NodeBase;
    }
}