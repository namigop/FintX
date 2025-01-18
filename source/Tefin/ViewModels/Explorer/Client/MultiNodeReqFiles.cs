using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Features;

namespace Tefin.ViewModels.Explorer.Client;

public class MultiNodeReqFiles(IExplorerItem[] items, ProjectTypes.ClientGroup client) : MultiNodeFile(items, client) {
    protected override FSharpResult<string, Exception> CreateZipShare(IOs io, string zipFile, string[] files, ProjectTypes.ClientGroup client) {
        var share = new SharingFeature();
        return share.ShareRequests(this.Io, zipFile, files, this._client);
    }

    public override void Init() { }
}