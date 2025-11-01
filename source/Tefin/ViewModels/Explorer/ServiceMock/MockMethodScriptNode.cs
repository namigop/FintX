using System.Windows.Input;

using Tefin.ViewModels.Tabs.Grpc;

namespace Tefin.ViewModels.Explorer.ServiceMock;

public class MockMethodScriptNode : FileNode {
    public MockMethodScriptNode(string fullPath) : base(fullPath) => this.CanOpen = true;

    //this.ExportCommand = this.CreateCommand(this.OnExport);
    public ICommand ExportCommand { get; }

    public override string Title {
        get => base.Title;
        set {
            base.Title = value;
            this.TempTitle = value;
        }
    }

    //TODO
    // private async Task OnExport() {
    //     var share = new SharingFeature();
    //     var zipName = $"{Path.GetFileNameWithoutExtension(this.FullPath)}_export.zip";
    //     var zipFile = await share.GetZipFile(zipName);
    //     if (string.IsNullOrEmpty(zipFile)) {
    //         return;
    //     }
    //
    //     var files = new[] { this.FullPath };
    //     var methodNode = this.FindParentNode<MockMethodNode>();
    //     var result = share.ShareRequests(this.Io, zipFile, files, methodNode!.Client);
    //     if (result.IsOk) {
    //         this.Io.Log.Info($"Export created: {zipFile}");
    //     }
    //     else {
    //         this.Io.Log.Error(result.ErrorValue);
    //     }
    // }

    public GrpcMockMethodHostViewModel? CreateViewModel() => ((MockMethodNode)this.Parent!).CreateViewModel();
}