namespace Tefin.Grpc.Dynamic

open System.Reflection
open Google.Protobuf.WellKnownTypes
open Tefin.Core
open Tefin.Core.Reflection

type SerParam = {
    Method:MethodInfo
    MethodParams : obj array
}

module DynamicTypes =
    let private emitUnaryRequestClass methodInfo = RequestUtils.emitRequestClass "GrpcUnary" methodInfo
    let private emitClientStreamingRequestClass methodInfo = RequestUtils.emitRequestClass "GrpcClientStreaming" methodInfo
    let private emitDuplexStreamingRequestClass methodInfo = RequestUtils.emitRequestClass "GrpcDuplexStreaming" methodInfo
    let private emitServerStreamingRequestClass methodInfo = RequestUtils.emitRequestClass "GrpcServerStreaming" methodInfo
    
     
    let toJson_unary (p:SerParam) =
        let genType = emitUnaryRequestClass p.Method
        let props = p.Method.GetParameters()
                       |> Array.map (fun p -> p.Name)
                       |> Array.zip p.MethodParams
                       |> Array.map (fun (i,name) -> { PropInfoName = name; Value = i })
        Instance.toJson props genType
    
    let fromJson_unary (method:MethodInfo) (json:string) =
        let genType = emitUnaryRequestClass method
        Instance.fromJson json genType
    
    

