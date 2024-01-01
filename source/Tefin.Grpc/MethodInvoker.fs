namespace Tefin.Grpc.Execution

open System
open System.Collections.Generic
open System.Reflection
open System.Threading.Tasks
open Tefin.Core.Reflection
open Grpc.Core
open Grpc.Core.Interceptors
open Tefin.Grpc.Custom

type TraceMessage =
    { StartTicks: int64
      Elapsed: TimeSpan
      Method: string

    }

module MethodInvoker =

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
        async {
            let call = mi.Invoke(instance, args)

            try
                return Some call
            with exc ->
                Console.WriteLine exc
                return Option<obj>.None
        }

    let private invokeAwaitable (mi: MethodInfo) (instance: obj) (args: obj array) =
        task {
            let call = mi.Invoke(instance, args)
            let task = call.GetType().GetProperty("ResponseAsync").GetValue(call) :?> Task
            do! task
            return Option<obj>.None
        }

    let private invokeVoid (mi: MethodInfo) (instance: obj) (args: obj array) =
        ignore (mi.Invoke(instance, args))
        Option<obj>.None

    let private invokeWithResult (mi: MethodInfo) (instance: obj) (args: obj array) =
        let result = mi.Invoke(instance, args)
        Some result

    let createGrpcClient  =
        let cacheConfig = Dictionary<Type, CallConfig>()
        let cache = Dictionary<Type, obj>()
        fun (clientType:Type) (cfg: CallConfig) (onErr:Exception->unit) ->
            if (cacheConfig.ContainsKey clientType) then
                let prevCfg = cacheConfig[clientType]
                let isConfigUnchanged =
                    prevCfg.Url = cfg.Url &&
                    prevCfg.X509Cert = cfg.X509Cert &&
                    prevCfg.JWT = cfg.JWT &&
                    prevCfg.IsUsingSSL = cfg.IsUsingSSL

                //if the config was changed clear the cached instance
                if not isConfigUnchanged then
                   let _ = cache.Remove(clientType)
                   ()

            if cache.ContainsKey clientType then
                cache[clientType]
            else
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", cfg.Url.StartsWith("http://"))

                //let clientType = methodInfo.DeclaringType
                let channel = (ChannelBuilder.createGrpcChannel cfg) :> ChannelBase
                let callInvoker = channel.Intercept(CallInterceptor(cfg.Url, cfg.Io, onErr))

                let clientInstance = Activator.CreateInstance(clientType, callInvoker)
                cache[clientType] <- clientInstance
                cacheConfig[clientType] <- cfg
                clientInstance

    let invoke (methodInfo: MethodInfo) (mParams: obj array) (cfg: CallConfig) (onErr:Exception->unit)=
        task {
            
            let client = createGrpcClient methodInfo.DeclaringType cfg onErr

            //if the return type is a Task
            if (TypeHelper.isOfType typeof<Task> methodInfo.ReturnType) then
                if (methodInfo.ReturnType.IsGenericType) then
                    return! invokeTaskWithResult methodInfo client mParams
                else
                    return! invokeTask methodInfo client mParams

            else if isAwaitable methodInfo then
                if (methodInfo.ReturnType.IsGenericType) then
                    return! (invokeAwaitableWithResult methodInfo client mParams)
                else
                    return! invokeAwaitable methodInfo client mParams
            else if methodInfo.ReturnType = typeof<unit> then
                return (invokeVoid methodInfo client mParams)
            else
                return (invokeWithResult methodInfo client mParams)
        }