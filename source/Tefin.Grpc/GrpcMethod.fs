namespace Tefin.Grpc

open System.Reflection
open Google.Protobuf.WellKnownTypes
open Grpc.Core
open Tefin.Core

module GrpcMethod =

  /// <summary>
  /// Classifies a service method, the server side of a gRPC service.
  /// </summary>
  /// <remarks>
  /// The shape of the method tells the four kinds apart. The parameter names cannot: a unary
  /// method and a server streaming method have the same parameter list, so matching on the
  /// names reported every server streaming method as unary.
  /// <para>
  /// The stream parameters are recognised by their type, not by their name. The types are
  /// compared by their generic type definition name, so a service assembly built against a
  /// different Grpc.Core.Api version is still recognised. Note that in F# a
  /// <c>typeof&lt;IAsyncStreamReader&lt;_&gt;&gt;</c> is a closed type over System.Object and not
  /// the generic type definition, which is why <c>typedefof</c> is used here.
  /// </para>
  /// </remarks>
  let getMethodTypeFromClient (mi:MethodInfo) =
    let parameters = mi.GetParameters()

    let requestStreamTypeName = typedefof<IAsyncStreamReader<_>>.FullName
    let responseStreamTypeName = typedefof<IServerStreamWriter<_>>.FullName

    let hasParameterOfType (typeName:string) =
      parameters
      |> Array.exists (fun p ->
             p.ParameterType.IsGenericType &&
             p.ParameterType.GetGenericTypeDefinition().FullName = typeName)

    if hasParameterOfType requestStreamTypeName then
      //the service reads a request stream
      if hasParameterOfType responseStreamTypeName then MethodType.Duplex
      else MethodType.ClientStreaming
    elif hasParameterOfType responseStreamTypeName then
      //the service writes a response stream
      MethodType.ServerStreaming
    elif mi.ReturnType.IsGenericType then
      //Task<TReply>, the service returns a single reply
      MethodType.Unary
    else
      //Task, the service streams the replies
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
