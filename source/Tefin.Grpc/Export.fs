namespace Tefin.Grpc

open System
open System.Reflection
open Tefin.Core
open Tefin.Grpc.Dynamic

module Export =
    
    let private requestToExportType (methodInfo:MethodInfo) (reqStreamOpt:obj option)=
        let requestTypRet = DynamicTypes.emitRequestClassForMethod(methodInfo)
        match reqStreamOpt with
            | Some r ->
                let streamType = r.GetType()
                requestTypRet
                |> Res.map (fun requestType -> 
                    let t = CoreExport.emitRequestExportClass (Some streamType) requestType
                    true, t)
            | None -> 
                requestTypRet
                |> Res.map (fun requestType -> 
                    let t = CoreExport.emitRequestExportClass None requestType
                    false, t)
        
    let private createExportInstance (methodInfo:MethodInfo) (requestStream:obj option) (reqInstance:obj) =        
        requestToExportType methodInfo requestStream
        |> Res.map (fun (isStreaming, exportType) ->
            let exportInstance = Activator.CreateInstance exportType
            let reqOrStreamTypeRet = GrpcMethod.getMethodRequestType methodInfo
            
            let methodType = GrpcMethod.getMethodType methodInfo
            exportType.GetProperty("Api").SetValue(exportInstance, GrpcPackage.packageName)
            exportType.GetProperty("Method").SetValue(exportInstance, methodInfo.Name)
            exportType.GetProperty("MethodType").SetValue(exportInstance, $"{methodType}" )
            exportType.GetProperty("RequestType").SetValue(exportInstance, (Res.getValue reqOrStreamTypeRet).FullName)
            exportType.GetProperty("Request").SetValue(exportInstance, reqInstance)
            
            if isStreaming then
                exportType.GetProperty("RequestStream").SetValue(exportInstance, requestStream.Value)
        
            exportInstance)
        
        
    let requestToJson (p:SerParam) =
        let reqType = Res.getValue (DynamicTypes.emitRequestClassForMethod(p.Method))
        let reqInstance =
                let temp =  Activator.CreateInstance reqType
                let props = CoreMethod.paramsToPropInfos p.Method p.RequestParams
                for pi in props do
                    reqType.GetProperty(pi.PropInfoName).SetValue(temp, pi.Value)
                temp
        let (isStreaming, exportType) = Res.getValue (requestToExportType p.Method p.RequestStream)
        let exportInstanceRet = createExportInstance p.Method p.RequestStream reqInstance
        
        exportInstanceRet
        |> Res.map (Instance.indirectSerialize exportType)
        
    
         


