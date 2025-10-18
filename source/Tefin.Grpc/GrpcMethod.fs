namespace Tefin.Grpc

open System.Reflection
open Google.Protobuf.WellKnownTypes
open Tefin.Core

module GrpcMethod =

  let getMethodTypeFromClient (mi:MethodInfo) =
    let paramNames = mi.GetParameters() |> Array.map (fun p -> p.Name)
    let request = ""
    let context = "context"
    let requestStream = "requestStream"
    let responseStream = "responseStream"
    
    if Array.length paramNames = 2 && paramNames[0] = request && paramNames[1] = context then
      MethodType.Unary
    elif Array.length paramNames = 2 && paramNames[0] = requestStream && paramNames[1] = context then
      MethodType.ClientStreaming
    elif Array.length paramNames = 3 && paramNames[0] = requestStream && paramNames[1] = responseStream && paramNames[2] = context then
      MethodType.Duplex
    else
      MethodType.ServerStreaming
      
  let getMethodType (mi: MethodInfo) =
    let returnType = mi.ReturnType
    if returnType.Name.StartsWith("AsyncDuplexStreaming") then
      MethodType.Duplex
    else if returnType.Name.StartsWith("AsyncClientStreaming") then
      MethodType.ClientStreaming
    else if returnType.Name.StartsWith("AsyncServerStreaming") then
      MethodType.ServerStreaming
    else
      MethodType.Unary

  let getMethodRequestType (methodInfo: MethodInfo) =
    match getMethodType methodInfo with
    | MethodType.Duplex -> methodInfo.ReturnType.GetGenericArguments().[0] |> Res.ok //stream req
    | MethodType.Unary -> methodInfo.GetParameters().[0].ParameterType |> Res.ok //request parameter
    | MethodType.ClientStreaming -> methodInfo.ReturnType.GetGenericArguments().[0] |> Res.ok //stream req
    | MethodType.ServerStreaming -> methodInfo.GetParameters().[0].ParameterType |> Res.ok //request parameter
    | _ -> Res.failed (failwith "unknown grpc method type")
