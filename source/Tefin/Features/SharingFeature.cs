using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Utils;

namespace Tefin.Features;

public class SharingFeature {
    public FSharpResult<string, Exception> ShareRequests(IOs io, string zipFile, string[] files,
        ProjectTypes.ClientGroup client) => Share.createFileShare(io, zipFile, files, client);

    public FSharpResult<string, Exception> ShareMethod(IOs io, string zipFile, string methodName,
        ProjectTypes.ClientGroup client) => Share.createFolderShare(io, zipFile, methodName, client);

    public FSharpResult<string, Exception>
        ShareClient(IOs io, string zipFile, ProjectTypes.ClientGroup client) =>
        Share.createClientShare(io, zipFile, client);

    public async Task<string> GetZipFile() {
        var fileName = "Export.zip";
        var fileTitle = "FintX (*.zip)";

        var zipFile = await DialogUtils.SelectFile("Export request", fileName, fileTitle, $"*{Ext.zipExt}");
        return zipFile;
    }
}