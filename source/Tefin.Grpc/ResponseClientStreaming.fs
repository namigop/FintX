namespace Tefin.Grpc.Execution

open System
open System.Reflection
open System.Threading.Tasks
open Grpc.Core
open Microsoft.FSharp.Control
open Tefin.Core.Execution
open Tefin.Core.Interop
open Tefin.Core

type ClientStreamingCallInfo =
    { ClientStreamItemType: Type
      ClientStreamType: Type
      RequestItemType: Type
      ResponseItemType: Type
      RequestStream: obj
      GetStatusMethodInfo: MethodInfo
      GetTrailersMethodInfo: MethodInfo
      WriteAsyncMethodInfo: MethodInfo
      CompleteAsyncMethodInfo: MethodInfo
      ResponseHeadersAsyncPropInfo: PropertyInfo
      CallResult: obj }

    member this.GetStatus() =
        this.GetStatusMethodInfo.Invoke(this.CallResult, null) :?> Status

    member this.GetTrailers() =
        this.GetTrailersMethodInfo.Invoke(this.CallResult, null) :?> Metadata

    member this.GetResponse() =
        task {
            let pi = this.ClientStreamType.GetProperty("ResponseAsync")
            let task = pi.GetValue(this.CallResult) :?> Task //Task<ResponseItemType)
            do! task
            let respType = typedefof<Task<_>>.MakeGenericType this.ResponseItemType
            let resultProperty = respType.GetProperty("Result")
            return resultProperty.GetValue(task)
        }

    member this.GetResponseHeaders() =
        this.ResponseHeadersAsyncPropInfo.GetValue(this.CallResult) :?> Task<Metadata>

type ClientStreamingCallResponse =
    { Headers: Metadata option
      Status: Status option
      Trailers: Metadata option
      WriteCompleted: bool
      CallInfo: ClientStreamingCallInfo }

    static member Empty() =
        { Headers = None
          Status = None
          Trailers = None
          WriteCompleted = false
          CallInfo = Unchecked.defaultof<ClientStreamingCallInfo> }

    member this.HasStatus = this.Status.IsSome

type OkayClientStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: ClientStreamingCallResponse }

type ErrorClientStreamingResponse =
    { MethodInfo: MethodInfo
      Context: Context
      Response: obj }

type ResponseClientStreaming =
    | Okay of OkayClientStreamingResponse
    | Error of ErrorClientStreamingResponse

    member x.OkayOrFailed() =
        match x with
        | Okay v -> struct (true, box v.Response, v.Context)
        | Error err -> struct (true, err.Response, err.Context)

module ClientStreamingResponse =
    let private wrapResponse (methodInfo: MethodInfo) (resp: obj) (isError: bool) =
        let args = methodInfo.ReturnType.GetGenericArguments()
        let requestItemType = args[0]
        let responseItemType = args[1]

        let clientStreamType =
            typedefof<AsyncClientStreamingCall<_, _>>
                .MakeGenericType(requestItemType, responseItemType)

        let requestStreamPropInfo = clientStreamType.GetProperty("RequestStream") //IClientStreamWriter<TRequest>
        let requestStream = requestStreamPropInfo.GetValue(resp)
        let requestStreamType = requestStream.GetType()
        //typedefof<IClientStreamWriter<_>>.MakeGenericType requestItemType

        let writeMethod = requestStreamType.GetMethod("WriteAsync", [| requestItemType |])
        let completeMethod = requestStreamType.GetMethod("CompleteAsync")

        let getStatusMethodInfo = clientStreamType.GetMethod("GetStatus")
        let getTrailersMethodInfo = clientStreamType.GetMethod("GetTrailers")

        let responseHeadersAsyncPropInfo =
            clientStreamType.GetProperty("ResponseHeadersAsync")

        { Headers = None
          Trailers = None
          Status = None
          WriteCompleted = false
          CallInfo =
            { ClientStreamItemType = requestItemType
              ClientStreamType = clientStreamType
              RequestItemType = requestItemType
              ResponseItemType = responseItemType
              CallResult = resp
              RequestStream = requestStream
              GetStatusMethodInfo = getStatusMethodInfo
              GetTrailersMethodInfo = getTrailersMethodInfo
              WriteAsyncMethodInfo = writeMethod
              CompleteAsyncMethodInfo = completeMethod
              ResponseHeadersAsyncPropInfo = responseHeadersAsyncPropInfo } }

    let completeCall (resp: ClientStreamingCallResponse) =
        let status = resp.CallInfo.GetStatus()
        let trailers = resp.CallInfo.GetTrailers()
        let d = resp.CallInfo.CallResult :?> IDisposable
        d.Dispose()

        { resp with
            Trailers = Some trailers
            Status = Some status }

    let getResponseHeader (resp: ClientStreamingCallResponse) =
        task {
            let! meta = resp.CallInfo.GetResponseHeaders() //prop.GetValue(okayResp.CallResult) :?> Task<Metadata>
            return { resp with Headers = Some meta }
        }

    let completeWrite (resp: ClientStreamingCallResponse) =
        task {
            do! (resp.CallInfo.CompleteAsyncMethodInfo.Invoke(resp.CallInfo.RequestStream, null) :?> Task)
            return { resp with WriteCompleted = true }
        }

    let getResponse (resp: ClientStreamingCallResponse) = resp.CallInfo.GetResponse()

    let toStandardCallResponse (resp: ClientStreamingCallResponse) =
        { Headers = resp.Headers
          Trailers = resp.Trailers
          Status = resp.Status }

    let write (resp: ClientStreamingCallResponse) (reqItem: obj) =
        (resp.CallInfo.WriteAsyncMethodInfo.Invoke(resp.CallInfo.RequestStream, [| reqItem |]) :?> Task)



    let create (methodInfo: MethodInfo) (ctx: Context) : ResponseClientStreaming =
        if ctx.Success then
            let w = wrapResponse methodInfo (Res.getValue ctx.Response) false

            let t: OkayClientStreamingResponse =
                { MethodInfo = methodInfo
                  Context = ctx
                  Response = w }

            Okay t
        else
            let err = Res.getError ctx.Response
            let w = wrapResponse methodInfo (ErrorResponse(Error = err.Message)) true

            let t: ErrorClientStreamingResponse =
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
