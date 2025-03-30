#region

using System.Reflection;

using Tefin.Core;
using Tefin.Features;
using Tefin.Utils;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public static class GrpcUiUtils {
    public static async Task ExportRequest(object?[] mParams, List<RequestVariable> variables, object reqStream, MethodInfo methodInfo, IOs io) {
        var feature = new ExportFeature(methodInfo, mParams, variables, reqStream);
        var exportReqJson = feature.Export();
        if (exportReqJson.IsOk) {
            var fileName = $"{methodInfo.Name}_req{Ext.requestFileExt}";
            await DialogUtils.SaveFile("Export request", fileName, exportReqJson.ResultValue, "FintX request",
                $"*{Ext.requestFileExt}");
        }
        else {
            io.Log.Error(exportReqJson.ErrorValue);
        }
    }

    public static async Task ImportRequest(IRequestEditorViewModel requestEditor, IListEditorViewModel listEditor,
        List<RequestVariable> envVariables,
        Type listType, MethodInfo methodInfo, IOs io) {
        var fileExtensions = new[] { $"*{Ext.requestFileExt}" };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            ImportRequest(requestEditor, listEditor, envVariables, listType, methodInfo, files[0], io);
        }
    }

    public static void ImportRequest(IRequestEditorViewModel requestEditor, IListEditorViewModel listEditor,
        List<RequestVariable>envVariables,
        Type listType, MethodInfo methodInfo, string file,
        IOs io) {
        var requestStream = Activator.CreateInstance(listType);
        var import = new ImportFeature(io, file, methodInfo, requestStream);
        //var (importReq, importReqStream) = import.Run();
        var importResult = import.Run();
        
        if (importResult.IsOk) {
            var methodParams = importResult.ResultValue.MethodParameters;
            requestEditor.Show(methodParams, envVariables);
            listEditor.Show(importResult.ResultValue.RequestStream);
        }
        else {
            io.Log.Error(importResult.ErrorValue);
        }
    }
}