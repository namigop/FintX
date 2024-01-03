namespace Tefin.Grpc.Execution

open System
open System.Collections.Generic
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Grpc.Core
open Microsoft.FSharp.Control
open Tefin.Core.Execution
open Tefin.Core.Interop
open Tefin.Core

type ServerStreamingCallInfo =
    { ServerStreamItemType: Type
      ServerStreamType: Type
      ResponseStreamType: Type
      MoveNextMethodInfo: MethodInfo
      GetStatusMethodInfo: MethodInfo
      GetTrailersMethodInfo: MethodInfo
      ResponseStreamPropInfo: PropertyInfo
      ResponseHeadersAsyncPropInfo: PropertyInfo
      CurrentPropInfo: PropertyInfo }

    member this.GetResponseStream(callResult: obj) =
        this.ResponseStreamPropInfo.GetValue callResult

    member this.GetCurrent(callResult: obj) =
        this.CurrentPropInfo.GetValue(this.GetResponseStream(callResult))

    member this.GetStatus(callResult: obj) =
        this.GetStatusMethodInfo.Invoke(callResult, null) :?> Status

    member this.GetTrailers(callResult: obj) =
        this.GetTrailersMethodInfo.Invoke(callResult, null) :?> Metadata

    member this.MoveNext(callResult: obj, token: CancellationToken) =
        this.MoveNextMethodInfo.Invoke(this.GetResponseStream(callResult), [| token |]) :?> Task<bool>

    member this.GetResponseHeaders(callResult: obj) =
        this.ResponseHeadersAsyncPropInfo.GetValue(callResult) :?> Task<Metadata>

type StandardCallResponse =
    { Headers: Metadata option
      Status: Status option
      Trailers: Metadata option }

type ServerStreamingCallResponse =
    { Headers: Metadata option
      Status: Status option
      Trailers: Metadata option
      CallResult: obj
      CallInfo: ServerStreamingCallInfo }

type OkayServerStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: ServerStreamingCallResponse }

type ErrorServerStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ResponseServerStreaming =
    | Okay of OkayServerStreamingResponse
    | Error of ErrorServerStreamingResponse

    member x.OkayOrFailed() =
        match x with
        | Okay v -> struct (true, box v.Response, v.Context)
        | Error err -> struct (true, err.Response, err.Context)

module ServerStreamingResponse =
    let private wrapResponse =
        let cache = Dictionary<Type, ServerStreamingCallInfo>()

        fun (methodInfo: MethodInfo) (resp: obj) (isError: bool) ->
            let itemType = methodInfo.ReturnType.GetGenericArguments().[0]

            let serverStreamType =
                typedefof<AsyncServerStreamingCall<_>>.MakeGenericType itemType

            let found, callInfo = cache.TryGetValue serverStreamType

            if found then
                { Headers = None
                  Trailers = None
                  Status = None
                  CallResult = resp
                  CallInfo = callInfo }
            else
                let responseStreamType = typedefof<IAsyncStreamReader<_>>.MakeGenericType itemType
                let responseStreamPropInfo = serverStreamType.GetProperty("ResponseStream")
                let currentPropInfo = responseStreamType.GetProperty("Current")
                let moveNextMethodInfo = responseStreamType.GetMethod("MoveNext")
                let getStatusMethodInfo = serverStreamType.GetMethod("GetStatus")
                let getTrailersMethodInfo = serverStreamType.GetMethod("GetTrailers")

                let responseHeadersAsyncPropInfo =
                    serverStreamType.GetProperty("ResponseHeadersAsync")

                let callInfo =
                    { ServerStreamItemType = itemType
                      ServerStreamType = serverStreamType
                      ResponseStreamType = responseStreamType
                      ResponseStreamPropInfo = responseStreamPropInfo
                      CurrentPropInfo = currentPropInfo
                      MoveNextMethodInfo = moveNextMethodInfo
                      GetStatusMethodInfo = getStatusMethodInfo
                      GetTrailersMethodInfo = getTrailersMethodInfo
                      ResponseHeadersAsyncPropInfo = responseHeadersAsyncPropInfo }

                cache[serverStreamType] <- callInfo

                { Headers = None
                  Trailers = None
                  Status = None
                  CallResult = resp
                  CallInfo = callInfo }

    let toStandardCallResponse (resp: ServerStreamingCallResponse) =
        { Headers = resp.Headers
          Trailers = resp.Trailers
          Status = resp.Status }

    let completeCall (resp: ServerStreamingCallResponse) =
        task {
            let status =
                try
                    resp.CallInfo.GetStatus(resp.CallResult)
                with exc ->
                    Status(StatusCode.Unknown, exc.Message)

            let trailers =
                try
                    resp.CallInfo.GetTrailers(resp.CallResult)
                with exc ->
                    Metadata()

            let d = resp.CallResult :?> IDisposable
            d.Dispose()

            return
                { resp with
                    Trailers = Some trailers
                    Status = Some status }
        }

    let getResponseHeader (okayResp: ServerStreamingCallResponse) =
        task {
            let! meta = okayResp.CallInfo.GetResponseHeaders(okayResp.CallResult)
            return { okayResp with Headers = Some meta }
        }

    let create (methodInfo: MethodInfo) (ctx: Context) : ResponseServerStreaming =

        if ctx.Success then
            let w = wrapResponse methodInfo (Res.getValue ctx.Response) false

            let t: OkayServerStreamingResponse =
                { MethodInfo = methodInfo
                  Context = ctx
                  Response = w }

            Okay t
        else
            let err = Res.getError ctx.Response
            let w = wrapResponse methodInfo (new ErrorResponse(Error = err.Message)) true

            let t: ErrorServerStreamingResponse =
                { MethodInfo = methodInfo
                  Context = ctx
                  Response = w }

            Error t
