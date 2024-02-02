using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Utils;

namespace Tefin.Features;

public class SharingFeature {
  
    public FSharpResult<string, Exception> ShareRequests(IOResolver io, string zipFile, string[] files, ProjectTypes.ClientGroup client) {
        return Share.createFileShare(io, zipFile,files, client);
    }

    public FSharpResult<string, Exception> ShareMethod(IOResolver io, string zipFile, string methodName, ProjectTypes.ClientGroup client) {
        return Share.createFolderShare(io, zipFile,methodName, client);
    }

    public FSharpResult<string, Exception> ShareClient(IOResolver io, string zipFile, ProjectTypes.ClientGroup client) {
        return Share.createClientShare(io, zipFile, client);
    }

    public async Task<string> GetZipFile() {
        var fileName = "Export.zip";
        var fileTitle = "FintX (*.zip)";

        var zipFile = await DialogUtils.SelectFile("Export request", fileName, fileTitle, $"*{Ext.zipExt}");
        return zipFile;
    }
}