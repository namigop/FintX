namespace Tefin.Core

open System
open Tefin.Core.Reflection

type ReqExport =
  { Api: string
    Method: string
    MethodType: string
    RequestType: string
    Request: obj
    Variables: RequestEnvVar array
    RequestStream: obj option }

module CoreExport =

  let singleReqPropInfos requestType =
    [| { IsMethod = false
         Name = "Api"
         Type = typeof<string> }
       { IsMethod = false
         Name = "Method"
         Type = typeof<string> }
       { IsMethod = false
         Name = "MethodType"
         Type = typeof<string> }
       { IsMethod = false
         Name = "RequestType"
         Type = typeof<string> }
       { IsMethod = false
         Name = "Request"
         Type = requestType } |]

  let streamingRequestPropInfos requestType requestStreamType =
    [| { IsMethod = false; Name = "RequestStream"; Type = requestStreamType }
       { IsMethod = false; Name = "Variables"; Type = typeof<RequestEnvVar array> }  |]
    |> Array.append (singleReqPropInfos requestType)

  let emitRequestExportClass (requestStreamTypeOpt: Type option) (requestType: Type) : Type =
    let getProperties () =
      match requestStreamTypeOpt with
      | Some requestStreamType -> streamingRequestPropInfos requestType requestStreamType
      | None ->
        [| { IsMethod = false; Name = "Variables"; Type = typeof<RequestEnvVar array> } |]
        |> Array.append(singleReqPropInfos requestType)

    let getClassName () = $"ImportExport_{requestType.Name}"
    RequestUtils.emitRequest getClassName getProperties

  let inspect (exportType: Type) (exportInstance: obj) =
    let toString o =
      if o = null then "null" else o.ToString()

    let api = exportType.GetProperty("Api").GetValue(exportInstance) |> toString
    let method = exportType.GetProperty("Method").GetValue(exportInstance) |> toString

    let methodType = exportType.GetProperty("MethodType").GetValue(exportInstance) |> toString
    let requestType = exportType.GetProperty("RequestType").GetValue(exportInstance) |> toString
    let request = exportType.GetProperty("Request").GetValue(exportInstance)
    let vars = exportType.GetProperty("Variables").GetValue(exportInstance) :?> RequestEnvVar array
    let requestStream =
      let prop = exportType.GetProperty("RequestStream")
      if prop = null then
        None
      else
        Some(prop.GetValue exportInstance)

    { Api = api
      Method = method
      MethodType = methodType
      RequestType = requestType
      Request = request
      Variables = vars
      RequestStream = requestStream }
