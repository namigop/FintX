namespace Tefin.Grpc.Dynamic

open System
open System.Reflection
//open Google.Protobuf.WellKnownTypes
open Tefin.Core
open Tefin.Core.Reflection
open Tefin.Grpc

type SerParam =
  { Method: MethodInfo
    RequestParams: obj array
    EnvVariables : AllVariables
    RequestStream: obj option }

  static member Create(m, r) =
    { Method = m
      RequestParams = r
      EnvVariables = AllVariables.Empty()
      RequestStream = None }
  static member WithStream (r: SerParam) list = { r with RequestStream = list }

module DynamicTypes =
  let private emitUnaryRequestClass methodInfo =
    RequestUtils.emitRequestClass "GrpcUnary" methodInfo

  let private emitClientStreamingRequestClass methodInfo =
    RequestUtils.emitRequestClass "GrpcClientStreaming" methodInfo

  let private emitDuplexStreamingRequestClass methodInfo =
    RequestUtils.emitRequestClass "GrpcDuplexStreaming" methodInfo

  let private emitServerStreamingRequestClass methodInfo =
    RequestUtils.emitRequestClass "GrpcServerStreaming" methodInfo

  let emitRequestClassForMethod (methodInfo: MethodInfo) =
    match GrpcMethod.getMethodType methodInfo with
    | MethodType.Duplex -> emitDuplexStreamingRequestClass methodInfo |> Res.ok
    | MethodType.Unary -> emitUnaryRequestClass methodInfo |> Res.ok
    | MethodType.ClientStreaming -> emitClientStreamingRequestClass methodInfo |> Res.ok
    | MethodType.ServerStreaming -> emitServerStreamingRequestClass methodInfo |> Res.ok
    | _ -> Res.failed (failwith "unknown grpc method type")

  let toMethodParams (methodInfo: MethodInfo) (reqType: Type) (reqInstance: obj) =
    methodInfo.GetParameters()
    |> Array.map (fun p ->
      let prop = reqType.GetProperty(p.Name)

      if not (prop = null) then
        let value = prop.GetValue reqInstance

        if not (value = null) then
          value
        else
          TypeHelper.getDefault p.ParameterType
      else
        let struct (ok, inst) = TypeBuilder.getDefault p.ParameterType true None 0
        inst)

  let toJsonRequest (p: SerParam) =
    let props = CoreMethod.paramsToPropInfos p.Method p.RequestParams

    emitRequestClassForMethod p.Method
    |> Res.map (Instance.toJson props)
    |> Res.getValue

  let fromJsonRequest (method: MethodInfo) (json: string) =
    emitRequestClassForMethod method
    |> Res.map (Instance.fromJson json)
    |> Res.getValue
