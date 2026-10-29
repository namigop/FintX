namespace Tefin.Grpc.Execution

open System
open System.Collections.Concurrent
open System.Reflection
open System.Threading.Tasks
open Tefin.Core.Reflection
open Grpc.Core
open Grpc.Core.Interceptors
open Grpc.Net.Client
open Tefin.Grpc.Custom

type TraceMessage =
  { StartTicks: int64
    Elapsed: TimeSpan
    Method: string

  }

module MethodInvoker =

  /// <summary>
  /// A cached gRPC client together with the channel that it owns. The channel holds an
  /// HttpClient and its connection pool, so it has to be released explicitly when the
  /// client is replaced.
  /// </summary>
  type private GrpcClientInstance =
    { Client: obj
      Channel: GrpcChannel }

  let private clientCache = ConcurrentDictionary<Type, GrpcClientInstance>()
  let private clientCacheConfig = ConcurrentDictionary<Type, CallConfig>()
  let private clientCacheGate = obj()

  let private disposeInstance (instance: GrpcClientInstance) =
    try
      instance.Channel.Dispose()
    with _ ->
      ()

  let private isConfigTheSame (prevCfg: CallConfig) (cfg: CallConfig) =
    prevCfg.Url = cfg.Url
    && (prevCfg.X509Cert = cfg.X509Cert)
    && (prevCfg.JWT = cfg.JWT)
    && (prevCfg.IsUsingSSL = cfg.IsUsingSSL)
    && (prevCfg.IsUsingHttp2 = cfg.IsUsingHttp2)
    && (prevCfg.IsUsingHttp3 = cfg.IsUsingHttp3)
    && (prevCfg.ValidateCertificate = cfg.ValidateCertificate)
    && (prevCfg.IsUsingNamedPipes = cfg.IsUsingNamedPipes)
    && (prevCfg.IsUsingUnixDomainSockets = cfg.IsUsingUnixDomainSockets)

  let private isAwaitable (mi: MethodInfo) =
    not (mi.ReturnType.GetMethod("GetAwaiter") = null)

  let private invokeTask (mi: MethodInfo) (instance: obj) (args: obj array) =
    task {
      let task = mi.Invoke(instance, args) :?> Task
      do! task
      return None
    }

  let private invokeTaskWithResult (mi: MethodInfo) (instance: obj) (args: obj array) =
    task {
      let task = mi.Invoke(instance, args) :?> Task
      do! task
      let resultProperty = mi.ReturnType.GetProperty("Result")
      return resultProperty.GetValue(task) |> Some
    }

  let private invokeAwaitableWithResult (mi: MethodInfo) (instance: obj) (args: obj array) =
    task {
      try
        let call = mi.Invoke(instance, args)
        return Some call
      with exc ->
        Console.WriteLine exc
        return Option<obj>.None
    }

  let private invokeAwaitable (mi: MethodInfo) (instance: obj) (args: obj array) =
    task {
      let call = mi.Invoke(instance, args)
      let task = call.GetType().GetProperty("ResponseAsync").GetValue(call) :?> Task
      let! _ = task
      return Option<obj>.None
    }

  let private invokeVoid (mi: MethodInfo) (instance: obj) (args: obj array) =
    ignore (mi.Invoke(instance, args))
    Option<obj>.None

  let private invokeWithResult (mi: MethodInfo) (instance: obj) (args: obj array) =
    let result = mi.Invoke(instance, args)
    Some result

  let private tryGetMatching (clientType: Type) (cfg: CallConfig) =
    match clientCache.TryGetValue clientType with
    | true, existing ->
      match clientCacheConfig.TryGetValue clientType with
      | true, prevCfg when isConfigTheSame prevCfg cfg -> Some existing
      | _ -> None
    | _ -> None

  let createGrpcClient (clientType: Type) (cfg: CallConfig) (onErr: Exception -> unit) =
    //fast path: the cached client was created with the same configuration
    match tryGetMatching clientType cfg with
    | Some existing -> existing.Client
    | None ->
      lock clientCacheGate (fun () ->
        //another thread may have created or replaced the client in the meantime
        match tryGetMatching clientType cfg with
        | Some existing -> existing.Client
        | None ->
          //the configuration was changed, so the cached client is replaced. Its channel
          //owns a connection pool that has to be released, otherwise every endpoint
          //change leaks a live socket set.
          match clientCache.TryRemove clientType with
          | true, old -> disposeInstance old
          | _ -> ()

          clientCacheConfig.TryRemove clientType |> ignore

          AppContext.SetSwitch(
            "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",
            cfg.Url.StartsWith("http://")
          )

          let channel = ChannelBuilder.createGrpcChannel cfg
          let callInvoker = channel.Intercept(CallInterceptor(cfg.Url, cfg.Io, onErr))

          let clientInstance = Activator.CreateInstance(clientType, callInvoker)
          clientCache[clientType] <- { Client = clientInstance; Channel = channel }
          clientCacheConfig[clientType] <- cfg
          clientInstance)

  /// <summary>
  /// Releases every cached gRPC channel and its connection pool. Call this when the
  /// application shuts down.
  /// </summary>
  let disposeGrpcClients () =
    lock clientCacheGate (fun () ->
      for instance in clientCache.Values do
        disposeInstance instance

      clientCache.Clear()
      clientCacheConfig.Clear())

  let invoke (methodInfo: MethodInfo) (mParams: obj array) (cfg: CallConfig) (onErr: Exception -> unit) =
    task {

      let client = createGrpcClient methodInfo.DeclaringType cfg onErr

      //if the return type is a Task
      if (TypeHelper.isOfType typeof<Task> methodInfo.ReturnType) then
        if methodInfo.ReturnType.IsGenericType then
          return! invokeTaskWithResult methodInfo client mParams
        else
          return! invokeTask methodInfo client mParams

      else if isAwaitable methodInfo then
        if methodInfo.ReturnType.IsGenericType then
          return! (invokeAwaitableWithResult methodInfo client mParams)
        else
          return! invokeAwaitable methodInfo client mParams
      else if methodInfo.ReturnType = typeof<unit> then
        return (invokeVoid methodInfo client mParams)
      else
        return (invokeWithResult methodInfo client mParams)
    }
