#region

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

#endregion

namespace Tefin.Views.Controls;

public class IconControl : TemplatedControl {
    public static readonly StyledProperty<string> IconProperty = AvaloniaProperty.Register<IconControl, string>(nameof(Icon), "", defaultBindingMode: BindingMode.TwoWay);

    public string Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    // public static readonly StyledProperty<double> WidthProperty = AvaloniaProperty.Register<IconControl, double>(nameof(Width), 24, defaultBindingMode: BindingMode.TwoWay);
    // public double Width {
    //     get => this.GetValue(WidthProperty);
    //     set => this.SetValue(WidthProperty, value);
    // }
}