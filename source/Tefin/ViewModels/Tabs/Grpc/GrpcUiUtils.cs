#region

using System.Reflection;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.FSharp.Core;

using Tefin.Core;
using Tefin.Core.Interop;
using Tefin.Core.Reflection;
using Tefin.Features;
using Tefin.Grpc;
using Tefin.Utils;
using Tefin.ViewModels.Types;

#endregion

namespace Tefin.ViewModels.Tabs.Grpc;

public static class GrpcUiUtils {
    public static async Task ExportRequest(object?[] mParams,
        List<VarDefinition> requestVariables, 
        List<VarDefinition> responseVariables,
        List<VarDefinition> requestStreamVariables,
        List<VarDefinition> responseStreamVariables,
        object reqStream,
        MethodInfo methodInfo,
        IOs io) {
        
        var feature = new ExportFeature(
            methodInfo,
            mParams, 
            requestVariables,
            responseVariables,
            requestStreamVariables,
            responseStreamVariables, 
            reqStream);
        
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

    public static async Task ImportRequest(
        IRequestEditorViewModel requestEditor,
        IListEditorViewModel listEditor,
        AllVariableDefinitions envVars,
        Type listType, 
        MethodInfo methodInfo,
        ProjectTypes.ClientGroup cg,
        IOs io) {
        var fileExtensions = new[] { $"*{Ext.requestFileExt}" };
        var (ok, files) = await DialogUtils.OpenFile("Open request file", "FintX request", fileExtensions);
        if (ok) {
            ImportRequest(requestEditor, listEditor, envVars, listType, methodInfo, cg, files[0], io);
        }
    }

    public static void ImportRequest(
        IRequestEditorViewModel requestEditor,
        IListEditorViewModel listEditor,
        AllVariableDefinitions envVars,
        Type listType, 
        MethodInfo methodInfo,
        ProjectTypes.ClientGroup cg,
        string file,
        IOs io) {

        static List<VarDefinition> Convert(List<RequestEnvVar> requestEnvVars) {
            return requestEnvVars
                .Where( t=> SystemType.getActualType(t.Type).Item1)
                .Select(t => new VarDefinition() {
                    Tag = t.Tag,
                    JsonPath = t.JsonPath,
                    TypeName = SystemType.getActualType(t.Type).Item2, 
                    Scope = t.Scope
                }).ToList();
        }
        var requestStream = Activator.CreateInstance(listType);
        var import = new ImportFeature(io, file, methodInfo, requestStream);
        //var (importReq, importReqStream) = import.Run();
        var importResult = import.Run();
        
        if (importResult.IsOk) {
            var methodParams = importResult.ResultValue.MethodParameters;
             
            envVars.RequestVariables.Clear();
            envVars.RequestVariables.AddRange(Convert(importResult.ResultValue.Variables.RequestVariables));
            envVars.ResponseVariables.Clear();
            envVars.ResponseVariables.AddRange(Convert(importResult.ResultValue.Variables.ResponseVariables));
            envVars.RequestStreamVariables.Clear();
            envVars.RequestStreamVariables.AddRange(Convert(importResult.ResultValue.Variables.RequestStreamVariables));
            envVars.ResponseStreamVariables.Clear();
            envVars.ResponseStreamVariables.AddRange(Convert(importResult.ResultValue.Variables.ResponseStreamVariables));
            
            requestEditor.Show(methodParams, envVars.RequestVariables, cg);
            
            if (FSharpOption<object>.get_IsSome(importResult.ResultValue.RequestStream)) {
                requestStream = importResult.ResultValue.RequestStream.Value;
            }
            
            listEditor.Show(requestStream!, envVars.RequestStreamVariables);
        }
        else {
            io.Log.Error(importResult.ErrorValue);
        }
    }
}