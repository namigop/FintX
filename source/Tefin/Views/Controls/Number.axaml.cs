#region

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

#endregion

namespace Tefin.Views.Controls;

public partial class Number : UserControl {
    public static readonly StyledProperty<decimal?> MaximumProperty = AvaloniaProperty.Register<Number, decimal?>(nameof(MaximumProperty));

    public static readonly StyledProperty<decimal?> MinimumProperty = AvaloniaProperty.Register<Number, decimal?>(nameof(MinimumProperty));

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Number, string>(nameof(Text), "0", defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<decimal?> ValueProperty =
                    AvaloniaProperty.Register<Number, decimal?>(nameof(Value), 0, defaultBindingMode: BindingMode.TwoWay, coerce: OnCoerceValue);

    public Number() {
        this.InitializeComponent();
        this.TextBox.TextChanged += this.OnTextChanged;
        //this.TextBox.KeyDown += this.OnKeyDown;
    }

    public decimal? Maximum {
        get => this.GetValue(MaximumProperty);
        set => this.SetValue(MaximumProperty, value);
    }

    public decimal? Minimum {
        get => this.GetValue(MinimumProperty);
        set => this.SetValue(MinimumProperty, value);
    }

    public string Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public decimal? Value {
        get => this.GetValue(ValueProperty);
        set => this.SetValue(ValueProperty, value);
    }

    private static decimal? OnCoerceValue(AvaloniaObject arg1, decimal? arg2) {
        var n = (Number)arg1;
        
        n.Text = arg2.ToString() ?? "0";
        return arg2;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e) {
        var s = (TextBox)sender!;
        if (string.IsNullOrWhiteSpace(s.Text)) {
            s.Text = "0";
            e.Handled = true;
            this.Value = 0;
        }
        else {
            if (decimal.TryParse(s.Text, out var d)) {
                if (this.Minimum.HasValue && d < this.Minimum.Value)
                    this.Value = this.Minimum.Value;
                else if (this.Maximum.HasValue && d > this.Maximum.Value)
                    this.Value = this.Maximum.Value;
                else
                    this.Value = d;
            }
        }
    }
}