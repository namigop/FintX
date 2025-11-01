#region

using System.Xml;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

#endregion

namespace Tefin.Views.Controls;

public partial class CsEditor : UserControl {
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<CsEditor, bool>(
            "IsReadOnly",
            false,
            false,
            BindingMode.TwoWay,
            null,
            OnCoerceIsReadOnly);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<CsEditor, string>(
            "Text",
            "",
            false,
            BindingMode.TwoWay,
            null,
            OnCoerceText);

    public CsEditor() {
        this.InitializeComponent();
        this.Editor.Document = new TextDocument { Text = "" };
        this.Editor.TextChanged += this.OnTextChanged;
        this.Editor.Options.EnableHyperlinks = false;

        this.DetachedFromVisualTree += this.JsonTextEditor_DetachedFromVisualTree;
        this.SetupSyntaxHighlighting();
    }

    public bool IsReadOnly {
        get => this.GetValue(IsReadOnlyProperty);
        set => this.SetValue(IsReadOnlyProperty, value);
    }

    public string Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }


    private void JsonTextEditor_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        var t = sender as CsEditor;
        t.Editor.TextChanged -= this.OnTextChanged;
    }

    private static bool OnCoerceIsReadOnly(AvaloniaObject d, bool arg2) {
        var sender = (CsEditor)d;
        sender.Editor.IsReadOnly = arg2;
        return arg2;
    }

    private static string OnCoerceText(AvaloniaObject d, string arg2) {
        var sender = (CsEditor)d;
        if (arg2 != sender.Editor.Text) {
            sender.Editor.Text = arg2;
        }

        return arg2;
    }

    private void OnTextChanged(object? sender, EventArgs e) => this.Text = this.Editor.Text;

    private void SetupSyntaxHighlighting() {
        using var resource =
            typeof(CsEditor).Assembly.GetManifestResourceStream("Tefin.Resources.csharp.xshd");
        if (resource != null) {
            using var reader = new XmlTextReader(resource);
            this.Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }
}