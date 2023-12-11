using System.Xml;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;

using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

using Tefin.Utils;

namespace Tefin.Views.Controls; 


public partial class JsonEditor : UserControl {
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<JsonEditor, string>(
            "Text",
            "",
            false,
            BindingMode.TwoWay,
            null,
            OnCoerceText);

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<JsonEditor, bool>(
            "IsReadOnly",
            false,
            false,
            BindingMode.TwoWay,
            null,
            OnCoerceIsReadOnly);


    //private TextEditor editor;
    private readonly CharFoldingStrategy _folding;
    private readonly DispatcherTimer _foldingTimer;
    private FoldingManager? _foldingManager;

    public JsonEditor() {
        this.InitializeComponent();
        this.Editor.Document = new TextDocument { Text = "" };
        this.Editor.TextChanged += this.OnTextChanged;
        this._folding = new CharFoldingStrategy('{', '}');
        this._foldingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        this._foldingTimer.Tick += this.FoldingTimer_Tick;
        this._foldingTimer.IsEnabled = false;

        this.Editor.Options.EnableHyperlinks = false;

        this.DetachedFromVisualTree += this.JsonTextEditor_DetachedFromVisualTree;
        this.SetupSyntaxHighlighting();
    }


    public string Text {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public bool IsReadOnly {
        get => this.GetValue(IsReadOnlyProperty);
        set => this.SetValue(IsReadOnlyProperty, value);
    }

    private void JsonTextEditor_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
        var t = sender as JsonEditor;
        t!._foldingTimer.Stop();
        t._foldingTimer.Tick -= this.FoldingTimer_Tick;
        t.Editor.TextChanged -= this.OnTextChanged;
    }

    private static bool OnCoerceIsReadOnly(AvaloniaObject d, bool arg2) {
        var sender = (JsonEditor)d;
        sender.Editor.IsReadOnly = arg2;
        return arg2;
    }


    private static string OnCoerceText(AvaloniaObject d, string arg2) {
        var sender = (JsonEditor)d;
        if (arg2 != sender.Editor.Text) {
            sender.Editor.Text = arg2;
            sender._foldingTimer.IsEnabled = true;
        }

        return arg2;
    }


    private void SetupSyntaxHighlighting() {
        using var resource =
            typeof(JsonEditor).Assembly.GetManifestResourceStream("Tefin.Resources.json.xshd");
        if (resource != null) {
            using var reader = new XmlTextReader(resource);
            this.Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
    }

    private void FoldingTimer_Tick(object? sender, EventArgs e) {
        if (this._foldingManager == null)
            this._foldingManager = FoldingManager.Install(this.Editor.TextArea);

        if (this._foldingManager != null && this.Editor.Document.TextLength > 0)
            this._folding.UpdateFoldings(this._foldingManager, this.Editor.Document);
    }

    private void OnTextChanged(object? sender, EventArgs e) {
        this.Text = this.Editor.Text;
    }
}
