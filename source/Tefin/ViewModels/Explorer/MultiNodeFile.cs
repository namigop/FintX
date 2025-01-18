using System.Windows.Input;

using AvaloniaEdit.Utils;

using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Features;

namespace Tefin.ViewModels.Explorer;

public abstract class MultiNodeFile : NodeBase {
    protected readonly ProjectTypes.ClientGroup _client;

    public MultiNodeFile(IExplorerItem[] items, ProjectTypes.ClientGroup client) {
        this._client = client; //items[0].FindParentNode<ClientRootNode>()!.Client;
        this.Items.AddRange(items);
        this.DeleteCommand = this.CreateCommand(this.OnDelete);
        this.ExportCommand = this.CreateCommand(this.OnExport);
    }

    public ICommand ExportCommand {
        get;
    }

    public ICommand DeleteCommand { get; }

    protected abstract FSharpResult<string, Exception> CreateZipShare(IOs io, string zipFile, string[] folders, ProjectTypes.ClientGroup client);

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipName = "export.zip";
        var zipFile = await share.GetZipFile(zipName);
        if (string.IsNullOrEmpty(zipFile)) {
            return;
        }

        var files = this.Items.Where(c => c is FileNode).Select(t => ((FileNode)t).FullPath).ToArray();

        var result = this.CreateZipShare(this.Io, zipFile, files, this._client);
        if (result.IsOk) {
            this.Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            this.Io.Log.Error(result.ErrorValue);
        }
    }

    private void OnDelete() {
        var files = this.Items.Where(c => c is FileNode).Select(t => ((FileNode)t).FullPath);

        foreach (var file in files) {
            this.Io.File.Delete(file);
        }
    }

    public override void Init() { }
}