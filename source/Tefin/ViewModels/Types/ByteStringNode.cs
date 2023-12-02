using System.Windows.Input;

using Google.Protobuf;
using Avalonia.Controls;
using ReactiveUI;

using Tefin.Utils;

namespace Tefin.ViewModels.Types;

public class ByteStringNode : TypeBaseNode {
    private readonly string _formattedTypeName = $"{{{nameof(ByteString)}}}";
    private readonly string? _og;
    private string _base64;
    private string _file;
    private bool _isFromFile;

    public ByteStringNode(string name, Type type, ITypeInfo propInfo, object instance, TypeBaseNode parent) : base(name, type, propInfo, instance, parent) {
        this._og = this.TypedValue?.ToBase64();
        this.OpenFileCommand = this.CreateCommand(this.OnOpenFile);
    }

    public string Base64 {
        get => this._base64;
        set => this.RaiseAndSetIfChanged(ref this._base64, value);
    }

    public string File {
        get => this._file;
        set => this.RaiseAndSetIfChanged(ref this._file, value);
    }

    public override string FormattedTypeName => this._formattedTypeName;

    public override string FormattedValue {
        get {
            var typedValue = this.TypedValue;
            if (typedValue != null) {
                var size = Core.Utils.printFileSize(typedValue.Length);
                return size;
            }

            return "null";
        }
    }

    public bool IsFromFile {
        get => this._isFromFile;
        set => this.RaiseAndSetIfChanged(ref this._isFromFile, value);
    }

    public ICommand OpenFileCommand { get; }
    public ByteString? TypedValue => this.Value as ByteString;

    public void CreateFromBase64String() {
        try {
            this.Value = ByteString.FromBase64(this.Base64);
        }
        catch (Exception exc) {
            //this.Value = ByteString.FromBase64(_og);
        }
    }

    public override void Init(Dictionary<string, int> processedTypeNames) {
        if (this.TypedValue != null) {
            this.Base64 = this.TypedValue.ToBase64();
            this.IsFromFile = false;
            this.File = "";
        }
    }

    public void Reset() {
        this.Init();
    }

    private async Task OnOpenFile() {
        var filter = new FileDialogFilter() { Name = "All files (*.*)" };
        filter.Extensions.Add("*");
        var (ok, files) = await DialogUtils.OpenFile(filter);
        if (ok) {
            this.File = files[0];
            var newByteString = await System.IO.File.OpenRead(this._file).Then(c => ByteString.FromStreamAsync(c));
            this.Base64 = newByteString.ToBase64();
            this.CreateFromBase64String();
        }
    }
}