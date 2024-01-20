#region

using System.Xml;

using Avalonia;
using Avalonia.Controls;

using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

using Tefin.ViewModels.Misc;

#endregion

namespace Tefin.Views.Misc;

public partial class OutputMiscView : UserControl {
    private OutputMiscViewModel? _vm;

    public OutputMiscView() {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
        this.SetupSyntaxHighlighting();
        this.Editor.TextChanged += this.OnTextChanged;
        this.DetachedFromVisualTree += this.OnDetached;
    }

    private void OnDataContextChanged(object? sender, EventArgs e) {
        var temp = ((OutputMiscView)sender!).DataContext;
        if (temp != null && this._vm == null) {
            this._vm = (OutputMiscViewModel)temp;
            this._vm.Editor.SetTarget(this.Editor);
        }
    }

    private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e) {
        this.Editor.TextChanged -= this.OnTextChanged;
    }

    private void OnTextChanged(object? sender, EventArgs e) {
        var editor = (TextEditor)sender!;
        if (string.IsNullOrEmpty(editor.Document.Text)) {
            if (this._vm != null) {
                this._vm.Editor.Clear();
            }
        }
    }

    private void SetupSyntaxHighlighting() {
        using var resource = typeof(OutputMiscView).Assembly.GetManifestResourceStream("Tefin.Resources.log.xshd");
        if (resource != null) {
            using var reader = new XmlTextReader(resource);
            this.Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }
}