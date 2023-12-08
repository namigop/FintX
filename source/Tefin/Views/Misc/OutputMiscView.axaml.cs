using System.Xml;

using Avalonia.Controls;

using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

using Tefin.ViewModels.Misc;

namespace Tefin.Views.Misc;

public partial class OutputMiscView : UserControl {
    private OutputMiscViewModel? _vm;

    public OutputMiscView() {
        this.InitializeComponent();
        this.DataContextChanged += this.OnDataContextChanged;
        this.SetupSyntaxHighlighting();
    }

    public OutputMiscViewModel? ViewModel {
        get {
            return this._vm;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e) {
        var temp = (((OutputMiscView)sender!)).DataContext;
        if (temp != null && this._vm == null) {
            this._vm = (OutputMiscViewModel)temp;
            this._vm.Editor.SetTarget(this.Editor);
        }
    }

    private void SetupSyntaxHighlighting() {
        using (var resource = typeof(OutputMiscView).Assembly.GetManifestResourceStream("Tefin.Resources.log.xshd")) {
            if (resource != null) {
                using (var reader = new XmlTextReader(resource)) {
                    this.Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
    }
}