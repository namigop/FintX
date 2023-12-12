namespace Tefin.Core

open System
open System.Reflection
open Tefin.Core.Reflection

module CoreExport =
    let emitRequestExportClass (requestStreamTypeOpt: Type option) (requestType: Type) : Type =
        let getProperties () =
            match requestStreamTypeOpt with
            | Some requestStreamType ->
                [|
                   { IsMethod = false; Name = "Api"; Type = typeof<string> }
                   { IsMethod = false; Name = "Method"; Type = typeof<string> }
                   { IsMethod = false; Name = "MethodType"; Type = typeof<string> }
                   { IsMethod = false; Name = "RequestType"; Type = typeof<string> }
                   { IsMethod = false; Name = "Request"; Type = requestType }
                   { IsMethod = false; Name = "RequestStream"; Type = requestStreamType }
                   
                   |]
            | None ->
                [|
                   { IsMethod = false; Name = "Api"; Type = typeof<string> }
                   { IsMethod = false; Name = "Method"; Type = typeof<string> }
                   { IsMethod = false; Name = "MethodType"; Type = typeof<string> }
                   { IsMethod = false; Name = "RequestType"; Type = typeof<string> }
                   { IsMethod = false; Name = "Request";  Type = requestType } |]

        let getClassName () = $"ImportExport_{requestType.Name}"
        RequestUtils.emitRequest getClassName getProperties

    