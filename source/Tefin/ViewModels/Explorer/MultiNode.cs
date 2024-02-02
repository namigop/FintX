using System.Collections.ObjectModel;
using System.Windows.Input;

using AvaloniaEdit.Utils;

using Tefin.Core.Interop;
using Tefin.Features;

namespace Tefin.ViewModels.Explorer;

public class MultiNode : NodeBase {
    private readonly ProjectTypes.ClientGroup _client;
    public MultiNode(IExplorerItem[] items) {
        this._client = items[0].FindParentNode<ClientNode>()!.Client;
        this.Items.AddRange(items);
        this.DeleteCommand = this.CreateCommand(OnDelete);
        this.ExportCommand = this.CreateCommand(OnExport);
    }

    public ICommand ExportCommand {
        get;
        
    }

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipFile = await share.GetZipFile();
        if (string.IsNullOrEmpty(zipFile))
            return;
        
        var files = this.Items
            .Where(c => c is FileNode)
            .Select(t => ((FileNode)t).FullPath)
            .ToArray();
            
        var result = share.ShareRequests(this.Io, zipFile, files, this._client);
        if (result.IsOk) {
            Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            Io.Log.Error(result.ErrorValue);
        }
    }

    private void OnDelete() {
        var files = this.Items
            .Where(c => c is FileNode)
            .Select(t => ((FileNode)t).FullPath);
            
        foreach (var file in files)
            this.Io.File.Delete(file);
    }

    public ICommand DeleteCommand { get; }

    public override void Init(){}
}