namespace Tefin.Grpc

open System.Reflection
open Tefin.Core

module GrpcMethod =

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
