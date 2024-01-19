namespace Tefin.Grpc

open System
open System.Reflection
open Tefin.Core
open Tefin.Grpc.Dynamic

module Export =

    let private requestToExportType (methodInfo: MethodInfo) (reqStreamOpt: obj option) =
        let requestTypRet = DynamicTypes.emitRequestClassForMethod (methodInfo)

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

    let private createExportInstance (methodInfo: MethodInfo) (requestStream: obj option) (reqInstance: obj) =
        requestToExportType methodInfo requestStream
        |> Res.map (fun (isStreaming, exportType) ->
            let exportInstance = Activator.CreateInstance exportType
            let reqOrStreamTypeRet = GrpcMethod.getMethodRequestType methodInfo

            let methodType = GrpcMethod.getMethodType methodInfo
            exportType.GetProperty("Api").SetValue(exportInstance, GrpcPackage.packageName)
            exportType.GetProperty("Method").SetValue(exportInstance, methodInfo.Name)
            exportType.GetProperty("MethodType").SetValue(exportInstance, $"{methodType}")

            exportType
                .GetProperty("RequestType")
                .SetValue(exportInstance, (Res.getValue reqOrStreamTypeRet).FullName)

            exportType.GetProperty("Request").SetValue(exportInstance, reqInstance)

            if isStreaming then
                exportType
                    .GetProperty("RequestStream")
                    .SetValue(exportInstance, requestStream.Value)

            exportInstance)

    let importReq (io: IOResolver) (p: SerParam) (file: string) =
        let json = io.File.ReadAllText file

        let validate (info: ReqExport) =
            let reqTypeRet = GrpcMethod.getMethodRequestType p.Method

            match reqTypeRet with
            | Error r -> Res.failed r
            | Ok reqType ->
                if not (info.Api = GrpcPackage.packageName) then
                    Res.failed (failwith "Invalid package name")
                elif not (info.Method = p.Method.Name) then
                    Res.failed (failwith $"Invalid method name. Got {info.Method} but was expecting {p.Method.Name}")
                elif not (info.MethodType = $"{GrpcMethod.getMethodType p.Method}") then
                    Res.failed (failwith $"Invalid methodType name. Got {info.MethodType} but was expecting {GrpcMethod.getMethodType p.Method}")
                elif not (info.RequestType = reqType.FullName) then
                    Res.failed (failwith $"Invalid request type. Got {info.RequestType} but was expecting {reqType.FullName}")
                elif (info.Request = null) then
                    Res.failed (failwith $"Invalid request.  Value cannot be null")
                else
                    Res.ok info

        let ret =
            let exp = requestToExportType p.Method p.RequestStream
            let (isStreaming, _) = Res.getValue exp

            exp
            |> Res.map (fun (_, exportType) ->
                let instance = Instance.indirectDeserialize exportType json
                let info = CoreExport.inspect exportType instance
                info)
            |> Res.bind validate
            |> Res.map (fun info ->
                let requestTyp = DynamicTypes.emitRequestClassForMethod (p.Method) |> Res.getValue
                let objArray = DynamicTypes.toMethodParams p.Method requestTyp info.Request

                if not isStreaming then
                    let a = Res.ok objArray
                    let b = Res.failed (Exception("not expected"))
                    struct (a, b)
                //struct ((Res.ok objArray), (Res.failed (failwith "not expected")))
                else
                    let reqStream = info.RequestStream.Value
                    struct ((Res.ok objArray), (Res.ok reqStream))
            //failwith "not yet supported for client stream and duplex"
            )
            |> Res.getValue

        ret

    let requestToJson (p: SerParam) =
        let reqType = Res.getValue (DynamicTypes.emitRequestClassForMethod (p.Method))

        let reqInstance =
            let temp = Activator.CreateInstance reqType
            let props = CoreMethod.paramsToPropInfos p.Method p.RequestParams

            for pi in props do
                reqType.GetProperty(pi.PropInfoName).SetValue(temp, pi.Value)

            temp

        let (isStreaming, exportType) =
            Res.getValue (requestToExportType p.Method p.RequestStream)

        let exportInstanceRet = createExportInstance p.Method p.RequestStream reqInstance

        exportInstanceRet |> Res.map (Instance.indirectSerialize exportType)
