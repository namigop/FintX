#region

using System.Reflection;

using Tefin.Core;
using Tefin.Features;
using Tefin.Utils;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public static class GrpcUiUtils {

    public static async Task ExportRequest(object?[] mParams, object reqStream, MethodInfo methodInfo, IOResolver io) {
        var feature = new ExportFeature(methodInfo, mParams, reqStream);
        var exportReqJson = feature.Export();
        if (exportReqJson.IsOk) {
            var fileName = $"{methodInfo.Name}_req{Ext.requestFileExt}";
            await DialogUtils.SaveFile("Export request", fileName, exportReqJson.ResultValue, "FintX request", $"*{Ext.requestFileExt}");
        }
        else {
            io.Log.Error(exportReqJson.ErrorValue);
        }
    }

    public static async Task ImportRequest(IRequestEditorViewModel requestEditor, IListEditorViewModel listEditor, Type listType, MethodInfo methodInfo, IOResolver io) {
        var fileExtensions = new[] {
            $"*{Ext.requestFileExt}"
        };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            ImportRequest(requestEditor, listEditor, listType, methodInfo, files[0], io);
        }
    }

    public static async Task ImportRequest(IRequestEditorViewModel requestEditor, IListEditorViewModel listEditor, Type listType, MethodInfo methodInfo, string file,
        IOResolver io) {
        var requestStream = Activator.CreateInstance(listType);
        var import = new ImportFeature(io, file, methodInfo, requestStream);
        var (importReq, importReqStream) = import.Run();
        if (importReq.IsOk) {
            var methodParams = importReq.ResultValue;
            requestEditor.Show(methodParams);
        }
        else {
            io.Log.Error(importReq.ErrorValue);
        }

        if (importReqStream.IsOk) {
            listEditor.Show(importReqStream.ResultValue);
        }
        else {
            io.Log.Error(importReqStream.ErrorValue);
        }
    }
}