#region

using System.Windows.Input;

using ReactiveUI;

using Avalonia.Controls;

using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Types;

public class ByteArrayNode : TypeBaseNode {
    private string _base64;
    private string _file;
    private bool _isFromFile;

    public ByteArrayNode(string name, Type type, ITypeInfo propInfo, object? instance, TypeBaseNode parent) : base(name, type, propInfo, instance, parent) {
        this.IsFromFile = false;
        this.File = "";
        this.Base64 = "";
        if (instance != null) this.Base64 = Convert.ToBase64String((byte[])instance);

        this.OpenFileCommand = this.CreateCommand(this.OnOpenFile);
    }

    public string Base64 {
        get => this._base64;
        set {
            var changed = this._base64 != value;
            this.RaiseAndSetIfChanged(ref this._base64, value);
            if (changed)
                this.CreateFromBase64String();
        }
    }

    public string File {
        get => this._file;
        set => this.RaiseAndSetIfChanged(ref this._file, value);
    }

    public override string FormattedValue {
        get {
            if (this.Value != null)
                return $"Length = {Core.Utils.printFileSize(((byte[])this.Value).Length)}";
            return "null";
        }
    }

    public bool IsFromFile {
        get => this._isFromFile;
        set => this.RaiseAndSetIfChanged(ref this._isFromFile, value);
    }

    public ICommand OpenFileCommand { get; }

    public void CreateFromBase64String() {
        try {
            this.Value = Convert.FromBase64String(this.Base64);
        }
        catch (Exception exc) {
            //this.Value = this.Value;
        }
    }

    public override void Init(Dictionary<string, int> _) {
        if (this.Value != null) {
            this.IsFromFile = false;
            this.File = "";
            this.Base64 = ((byte[])this.Value).Then(Core.Utils.bytesToBase64);
        }
    }

    public void Reset() {
        this.Init();
    }

    //public ICommand OpenFileCommand { get; }
    protected override void OnValueChanged(object oldValue, object newValue) {
    }

    private async Task OnOpenFile() {
        var (ok, files) = await DialogUtils.OpenFile("Open File", "All Files", new[] { "*.*" });
        if (ok) {
            this.File = files[0];
            var bytes = System.IO.File.ReadAllBytes(this._file);
            this.Base64 = Convert.ToBase64String(bytes);
        }
    }
}