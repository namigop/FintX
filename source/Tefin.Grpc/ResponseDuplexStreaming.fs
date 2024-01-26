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

type DuplexStreamingCallInfo =
  { DuplexStreamType: Type
    RequestItemType: Type
    ResponseItemType: Type
    GetStatusMethodInfo: MethodInfo
    GetTrailersMethodInfo: MethodInfo
    WriteAsyncMethodInfo: MethodInfo
    MoveNextMethodInfo: MethodInfo
    CompleteAsyncMethodInfo: MethodInfo
    ResponseHeadersAsyncPropInfo: PropertyInfo
    RequestStreamPropInfo: PropertyInfo
    ResponseStreamPropInfo: PropertyInfo
    CurrentPropInfo: PropertyInfo }

  member this.GetStatus(callResult: obj) =
    this.GetStatusMethodInfo.Invoke(callResult, null) :?> Status

  member this.GetTrailers(callResult: obj) =
    this.GetTrailersMethodInfo.Invoke(callResult, null) :?> Metadata

  member this.GetCurrent(responseStream: obj) =
    this.CurrentPropInfo.GetValue(responseStream)

  member this.MoveNext(responseStream: obj, token: CancellationToken) =
    this.MoveNextMethodInfo.Invoke(responseStream, [| token |]) :?> Task<bool>

  member this.GetResponseHeaders(callResult: obj) =
    this.ResponseHeadersAsyncPropInfo.GetValue(callResult) :?> Task<Metadata>

type DuplexStreamingCallResponse =
  { Headers: Metadata option
    Status: Status option
    Trailers: Metadata option
    CallInfo: DuplexStreamingCallInfo
    RequestStream: obj
    ResponseStream: obj
    CallResult: obj
    WriteCompleted: bool }

  static member Empty() =
    { Headers = None
      Status = None
      Trailers = None
      RequestStream = Unchecked.defaultof<obj>
      ResponseStream = Unchecked.defaultof<obj>
      CallResult = Unchecked.defaultof<obj>
      CallInfo = Unchecked.defaultof<DuplexStreamingCallInfo>
      WriteCompleted = false }

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
  let private wrapResponse =
    let cache = Dictionary<Type, DuplexStreamingCallInfo>()

    fun (methodInfo: MethodInfo) (resp: obj) (err: ErrorResponse option) ->

      let args = methodInfo.ReturnType.GetGenericArguments()
      let requestItemType = args[0]
      let responseItemType = args[1]
      let isError = err.IsSome

      let duplexStreamType =
        typedefof<AsyncDuplexStreamingCall<_, _>>
          .MakeGenericType(requestItemType, responseItemType)

      let callInfo =
        let found, temp = cache.TryGetValue duplexStreamType

        if found then
          temp
        else

          let requestStreamPropInfo = duplexStreamType.GetProperty("RequestStream") //IDuplexStreamWriter<TRequest>
          let requestStream = requestStreamPropInfo.GetValue(resp)

          let requestStreamType =
            if isError then
              typeof<Exception>
            else
              requestStream.GetType()

          let responseStreamType =
            typedefof<IAsyncStreamReader<_>>.MakeGenericType responseItemType

          let responseStreamPropInfo = duplexStreamType.GetProperty("ResponseStream")
          //let responseStream = responseStreamPropInfo.GetValue(resp)
          let currentPropInfo = responseStreamType.GetProperty("Current")
          let moveNextMethodInfo = responseStreamType.GetMethod("MoveNext")

          let writeMethod = requestStreamType.GetMethod("WriteAsync", [| requestItemType |])
          let completeMethod = requestStreamType.GetMethod("CompleteAsync")

          let getStatusMethodInfo = duplexStreamType.GetMethod("GetStatus")
          let getTrailersMethodInfo = duplexStreamType.GetMethod("GetTrailers")

          let responseHeadersAsyncPropInfo =
            duplexStreamType.GetProperty("ResponseHeadersAsync")

          let temp =
            { DuplexStreamType = duplexStreamType
              RequestItemType = requestItemType
              ResponseItemType = responseItemType
              GetStatusMethodInfo = getStatusMethodInfo
              GetTrailersMethodInfo = getTrailersMethodInfo
              WriteAsyncMethodInfo = writeMethod
              CompleteAsyncMethodInfo = completeMethod
              MoveNextMethodInfo = moveNextMethodInfo
              RequestStreamPropInfo = requestStreamPropInfo
              ResponseStreamPropInfo = responseStreamPropInfo

              CurrentPropInfo = currentPropInfo
              ResponseHeadersAsyncPropInfo = responseHeadersAsyncPropInfo }

          cache[duplexStreamType] <- temp
          temp


      let requestStream =
        if isError then
          box Unchecked.defaultof<Exception>
        else
          callInfo.RequestStreamPropInfo.GetValue(resp)

      let responseStream =
        if isError then
          box Unchecked.defaultof<Exception>
        else
          callInfo.ResponseStreamPropInfo.GetValue(resp)

      { Headers = None
        Trailers = None
        Status = None
        CallResult = resp
        RequestStream = requestStream
        ResponseStream = responseStream
        WriteCompleted = isError
        CallInfo = callInfo }

  let completeCall (resp: DuplexStreamingCallResponse) =
    task {
      let d = resp.CallResult :?> IDisposable
      d.Dispose()

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



      return
        { resp with
            Trailers = Some trailers
            Status = Some status }
    }

  let getResponseHeader (resp: DuplexStreamingCallResponse) =
    task {
      try
        let! meta = resp.CallInfo.GetResponseHeaders(resp.CallResult) //prop.GetValue(okayResp.CallResult) :?> Task<Metadata>
        return { resp with Headers = Some meta }
      with exc ->
        return { resp with Headers = Some(Metadata()) }
    }

  let completeWrite (resp: DuplexStreamingCallResponse) =
    task {
      try
        do! (resp.CallInfo.CompleteAsyncMethodInfo.Invoke(resp.RequestStream, null) :?> Task)
        return { resp with WriteCompleted = true }
      with exc ->
        return { resp with WriteCompleted = true }
    }

  let toStandardCallResponse (resp: DuplexStreamingCallResponse) =
    { Headers = resp.Headers
      Trailers = resp.Trailers
      Status = resp.Status }

  let write (resp: DuplexStreamingCallResponse) (reqItem: obj) =
    task { do! (resp.CallInfo.WriteAsyncMethodInfo.Invoke(resp.RequestStream, [| reqItem |]) :?> Task) }

  let create (methodInfo: MethodInfo) (ctx: Context) : ResponseDuplexStreaming =

    if ctx.Success then
      let w = wrapResponse methodInfo (Res.getValue ctx.Response) None

      let t: OkayDuplexStreamingResponse =
        { MethodInfo = methodInfo
          Context = ctx
          Response = w }

      Okay t
    else
      let err = ctx.GetError()

      let w =
        wrapResponse methodInfo (Res.getValue ctx.Response) (new ErrorResponse(Error = err.Message) |> Some)

      let t: ErrorDuplexStreamingResponse =
        { MethodInfo = methodInfo
          Context = ctx
          Response = w }

      Error t
