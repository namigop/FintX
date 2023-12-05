namespace Tefin.Grpc.Execution

open System
open System.Reflection
open System.Threading
open System.Threading.Tasks
open Grpc.Core
open Microsoft.FSharp.Control
open Tefin.Core.Execution
open Tefin.Core.Interop
open Tefin.Core

type DuplexStreamingCallInfo =
    { DuplexStreamType: Type
      RequestItemType: Type
      ResponseItemType: Type
      RequestStream: obj
      GetStatusMethodInfo: MethodInfo
      GetTrailersMethodInfo: MethodInfo
      WriteAsyncMethodInfo: MethodInfo
      MoveNextMethodInfo: MethodInfo
      CompleteAsyncMethodInfo: MethodInfo
      ResponseHeadersAsyncPropInfo: PropertyInfo
      CurrentPropInfo: PropertyInfo
      ResponseStream: obj

      CallResult: obj }

    member this.GetStatus() =
        this.GetStatusMethodInfo.Invoke(this.CallResult, null) :?> Status

    member this.GetTrailers() =
        this.GetTrailersMethodInfo.Invoke(this.CallResult, null) :?> Metadata

    member this.GetCurrent() =
        this.CurrentPropInfo.GetValue(this.ResponseStream)

    member this.MoveNext(token: CancellationToken) =
        this.MoveNextMethodInfo.Invoke(this.ResponseStream, [| token |]) :?> Task<bool>

    member this.GetResponseHeaders() =
        this.ResponseHeadersAsyncPropInfo.GetValue(this.CallResult) :?> Task<Metadata>

type DuplexStreamingCallResponse =
    { Headers: Metadata option
      Status: Status option
      Trailers: Metadata option
      CallInfo: DuplexStreamingCallInfo }

    static member Empty() =
        { Headers = None
          Status = None
          Trailers = None
          CallInfo = Unchecked.defaultof<DuplexStreamingCallInfo> }

    member this.HasStatus = this.Status.IsSome

type OkayDuplexStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: DuplexStreamingCallResponse }

type ErrorDuplexStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ResponseDuplexStreaming =
    | Okay of OkayDuplexStreamingResponse
    | Error of ErrorDuplexStreamingResponse

    member x.OkayOrFailed() =
        match x with
        | Okay v -> struct (true, box v.Response, v.Context)
        | Error err -> struct (true, err.Response, err.Context)

module DuplexStreamingResponse =
    let private wrapResponse (methodInfo: MethodInfo) (resp: obj) (isError: bool) =
        let args = methodInfo.ReturnType.GetGenericArguments()
        let requestItemType = args[0]
        let responseItemType = args[1]

        let duplexStreamType =
            typedefof<AsyncDuplexStreamingCall<_, _>>
                .MakeGenericType(requestItemType, responseItemType)

        let requestStreamPropInfo = duplexStreamType.GetProperty("RequestStream") //IDuplexStreamWriter<TRequest>
        let requestStream = requestStreamPropInfo.GetValue(resp)
        let requestStreamType = requestStream.GetType()

        let responseStreamType =
            typedefof<IAsyncStreamReader<_>>.MakeGenericType responseItemType

        let responseStreamPropInfo = duplexStreamType.GetProperty("ResponseStream")
        let responseStream = responseStreamPropInfo.GetValue(resp)
        let currentPropInfo = responseStreamType.GetProperty("Current")
        let moveNextMethodInfo = responseStreamType.GetMethod("MoveNext")

        let writeMethod = requestStreamType.GetMethod("WriteAsync", [| requestItemType |])
        let completeMethod = requestStreamType.GetMethod("CompleteAsync")

        let getStatusMethodInfo = duplexStreamType.GetMethod("GetStatus")
        let getTrailersMethodInfo = duplexStreamType.GetMethod("GetTrailers")

        let responseHeadersAsyncPropInfo =
            duplexStreamType.GetProperty("ResponseHeadersAsync")

        { Headers = None
          Trailers = None
          Status = None
          CallInfo =
            { DuplexStreamType = duplexStreamType
              RequestItemType = requestItemType
              ResponseItemType = responseItemType
              CallResult = resp
              RequestStream = requestStream
              GetStatusMethodInfo = getStatusMethodInfo
              GetTrailersMethodInfo = getTrailersMethodInfo
              WriteAsyncMethodInfo = writeMethod
              CompleteAsyncMethodInfo = completeMethod
              MoveNextMethodInfo = moveNextMethodInfo
              ResponseStream = responseStream
              CurrentPropInfo = currentPropInfo
              ResponseHeadersAsyncPropInfo = responseHeadersAsyncPropInfo } }

    let completeCall (resp: DuplexStreamingCallResponse) =
        let status = resp.CallInfo.GetStatus()
        let trailers = resp.CallInfo.GetTrailers()
        let d = resp.CallInfo.CallResult :?> IDisposable
        d.Dispose()

        { resp with
            Trailers = Some trailers
            Status = Some status }

    let getResponseHeader (resp: DuplexStreamingCallResponse) =
        task {
            let! meta = resp.CallInfo.GetResponseHeaders() //prop.GetValue(okayResp.CallResult) :?> Task<Metadata>
            return { resp with Headers = Some meta }
        }

    let completeWrite (resp: DuplexStreamingCallResponse) =
        (resp.CallInfo.CompleteAsyncMethodInfo.Invoke(resp.CallInfo.RequestStream, null) :?> Task)

    let toStandardCallResponse (resp: DuplexStreamingCallResponse) =
        { Headers = resp.Headers
          Trailers = resp.Trailers
          Status = resp.Status }

    let write (resp: DuplexStreamingCallResponse) (reqItem: obj) =
        (resp.CallInfo.WriteAsyncMethodInfo.Invoke(resp.CallInfo.RequestStream, [| reqItem |]) :?> Task)

    let create (methodInfo: MethodInfo) (ctx: Context) : ResponseDuplexStreaming =

        if ctx.Success then
            let w = wrapResponse methodInfo (Res.getValue ctx.Response) false

            let t: OkayDuplexStreamingResponse =
                { MethodInfo = methodInfo
                  Context = ctx
                  Response = w }

            Okay t
        else
            let err = Res.getError ctx.Response

            let w = wrapResponse methodInfo (ErrorResponse(Error = err.Message)) true

            let t: ErrorDuplexStreamingResponse =
                { MethodInfo = methodInfo
                  Context = ctx
                  Response = w }

            Error t

// let createJson (resp: ResponseClientStreaming) =
//     match resp with
//     | Okay k ->
//         let isError = false
//         wrapResponse k.MethodInfo k.Response isError
//     | Error e ->
//         let isError = true
//         wrapResponse e.MethodInfo e.Response isError
