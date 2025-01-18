using System.Windows.Input;

using Tefin.Features;
using Tefin.ViewModels.Explorer.Client;

namespace Tefin.ViewModels.Explorer;

public class FileReqNode : FileNode {
    public FileReqNode(string fullPath) : base(fullPath) {
        this.CanOpen = true;
        this.ExportCommand = this.CreateCommand(this.OnExport);
    }

    public ICommand ExportCommand { get; }

    public override string Title {
        get => base.Title;
        set {
            base.Title = value;
            this.TempTitle = value;
        }
    }

    private async Task OnExport() {
        var share = new SharingFeature();
        var zipName = $"{Path.GetFileNameWithoutExtension(this.FullPath)}_export.zip";
        var zipFile = await share.GetZipFile(zipName);
        if (string.IsNullOrEmpty(zipFile)) {
            return;
        }

        var files = new[] { this.FullPath };
        var methodNode = this.FindParentNode<MethodNode>();
        var result = share.ShareRequests(this.Io, zipFile, files, methodNode!.Client);
        if (result.IsOk) {
            this.Io.Log.Info($"Export created: {zipFile}");
        }
        else {
            this.Io.Log.Error(result.ErrorValue);
        }
    }

    public ClientMethodViewModelBase? CreateViewModel() => ((MethodNode)this.Parent!).CreateViewModel();
}