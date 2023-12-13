using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Tefin.Resources; 

public class IconTemplate : IDataTemplate {
    [Content]
    public Dictionary<string, IDataTemplate> AllIcons { get; } = new();
    
    public Control? Build(object? param) {
        if (param == null) {
            return new Border() {
                Width = 24,
                Height = 24,
                Background = new SolidColorBrush(Colors.CornflowerBlue),
                CornerRadius = new CornerRadius(4)
            };
        }

        return AllIcons[param.ToString()!].Build(param);
    }

    public bool Match(object? data) {
        return data is string;
    }
}