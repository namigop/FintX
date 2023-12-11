namespace Tefin.Grpc.Execution

open System
open System.Collections.Generic
open System.Reflection
open Grpc.Core
open Tefin.Core.Execution
open Tefin.Core.Interop
open System.Threading.Tasks
open Tefin.Core
open Tefin.Core.Reflection

type OkayUnaryResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ErrorUnaryResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ResponseUnary =
    | Okay of OkayUnaryResponse
    | Error of ErrorUnaryResponse

    member x.OkayOrFailed() =
        match x with
        | Okay v -> struct (true, v.Response, v.Context)
        | Error err -> struct (true, err.Response, err.Context)

type AsyncUnaryCallInfo =
    { ResponseHeadersPropInfo: PropertyInfo
      GetStatusMethodInfo: MethodInfo
      GetTrailersMethodInfo: MethodInfo }

module UnaryResponse =

    let awaitCall (methodInfo: MethodInfo) (resp2: obj) =
        task {
            let responseProperty = methodInfo.ReturnType.GetProperty("ResponseAsync")
            let task = responseProperty.GetValue(resp2) :?> Task
            do! task
            let resultProperty = task.GetType().GetProperty("Result")
            return resultProperty.GetValue(task)
        }

    let completeAsyncCall  =
        let cache = Dictionary<Type, AsyncUnaryCallInfo>()
        fun (methodInfo: MethodInfo) (resp: obj) (wrapperType: Type) (wrapperInst: obj) ->
            task {
                let itemType = methodInfo.ReturnType.GetGenericArguments().[0]
                let asyncUnaryCallType = typedefof<AsyncUnaryCall<_>>.MakeGenericType itemType
                let callInfo =
                    let found, temp = cache.TryGetValue asyncUnaryCallType
                    if found then temp
                    else
                         let respHeadersPi = methodInfo.ReturnType.GetProperty("ResponseHeadersAsync")
                         let getStatusMi = methodInfo.ReturnType.GetMethod("GetStatus")
                         let getTrailersMi = methodInfo.ReturnType.GetMethod("GetTrailers")
                         let temp = { ResponseHeadersPropInfo = respHeadersPi
                                      GetStatusMethodInfo = getStatusMi
                                      GetTrailersMethodInfo = getTrailersMi }
                         cache[asyncUnaryCallType] <- temp
                         temp

                let unaryAsyncCall = resp
                let! actualResponse = awaitCall methodInfo unaryAsyncCall               
                let responsePi = wrapperType.GetProperty("Response")
                responsePi.SetValue(wrapperInst, actualResponse)
                
                let! headers = callInfo.ResponseHeadersPropInfo.GetValue(unaryAsyncCall) :?> Task<Metadata>
                let trailers = callInfo.GetTrailersMethodInfo.Invoke(unaryAsyncCall, null) :?> Metadata
                let status = callInfo.GetStatusMethodInfo.Invoke(unaryAsyncCall, null) :?> Status
               
                wrapperType.GetProperty("Headers").SetValue(wrapperInst, headers)
                wrapperType.GetProperty("Trailers").SetValue(wrapperInst, trailers)
                wrapperType.GetProperty("Status").SetValue(wrapperInst, status)
                (unaryAsyncCall :?> IDisposable).Dispose()
                return wrapperInst
            }

    let private wrapResponse (methodInfo: MethodInfo) (resp: obj) (isError: bool) =
        task {
            let isAsync = methodInfo.ReturnType.IsGenericType
            let wrapperType = ResponseUtils.emitUnaryResponse methodInfo isAsync isError
            let wrapperInst = Activator.CreateInstance(wrapperType)

            if isAsync then
                return! completeAsyncCall methodInfo resp wrapperType wrapperInst
            else
                let prop = wrapperType.GetProperty("Response")
                prop.SetValue(wrapperInst, resp)
                return wrapperInst
        }

    let create (methodInfo: MethodInfo) (ctx: Context) =
        task {
            if ctx.Success then
                let! w = wrapResponse methodInfo (Res.getValue ctx.Response) false

                let t: OkayUnaryResponse =
                    { MethodInfo = methodInfo
                      Context = ctx
                      Response = w }

                return (Okay t)
            else
                let err = Res.getError ctx.Response

                let! w = wrapResponse methodInfo (ErrorResponse(Error = err.Message)) true

                let t: ErrorUnaryResponse =
                    { MethodInfo = methodInfo
                      Context = ctx
                      Response = w }

                return (Error t)
        }

    let createJson (resp: ResponseUnary) =
        match resp with
        | Okay k ->
            let isError = false
            wrapResponse k.MethodInfo k.Response isError
        | Error e ->
            let isError = true
            wrapResponse e.MethodInfo e.Response isError
