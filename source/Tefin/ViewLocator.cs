#region

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using Tefin.ViewModels;

#endregion

namespace Tefin;

public class ViewLocator : IDataTemplate {
    public Control Build(object? data) {
        if (data == null) {
            return new TextBlock { Text = "data cannot be null" };
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null) {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}