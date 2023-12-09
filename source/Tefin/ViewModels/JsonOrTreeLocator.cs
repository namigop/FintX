using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

using Tefin.Utils;
using Tefin.ViewModels.Tabs;

namespace Tefin.ViewModels;

public class JsonOrTreeRequestLocator : IDataTemplate {
    
    [Content]
    public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new();

    public Control Build(object? data) {
        if (data == null)
            return new TextBlock { Text = "data cannot be null"};
        
        dynamic vm = data;
        return
            (vm.ShowTreeEditor
                ? this.AvailableTemplates["AsTree"]
                : this.AvailableTemplates["AsJson"])
            .Then(d => d.Build(data))!;

    }

    public bool Match(object? data) {
        return data is IRequestEditorViewModel;

    }

}