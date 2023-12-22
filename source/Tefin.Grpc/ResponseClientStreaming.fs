namespace Tefin.Grpc.Execution

open System
open System.Collections.Generic
open System.Reflection
open System.Threading.Tasks
open Grpc.Core
open Microsoft.FSharp.Control
open Tefin.Core.Execution
open Tefin.Core.Interop
open Tefin.Core
open Tefin.Core.Reflection

type ClientStreamingCallInfo =
    { ClientStreamItemType: Type
      ClientStreamType: Type
      RequestItemType: Type
      ResponseItemType: Type
      GetStatusMethodInfo: MethodInfo
      GetTrailersMethodInfo: MethodInfo
      WriteAsyncMethodInfo: MethodInfo
      CompleteAsyncMethodInfo: MethodInfo
      ResponseHeadersAsyncPropInfo: PropertyInfo
      RequestStreamPropInfo: PropertyInfo
      Method : MethodInfo}

    member this.GetStatus(callResult: obj) =
        this.GetStatusMethodInfo.Invoke(callResult, null) :?> Status

    member this.GetTrailers(callResult: obj) =
        this.GetTrailersMethodInfo.Invoke(callResult, null) :?> Metadata

    member this.GetResponse(callResult: obj) =
        task {
            let pi = this.ClientStreamType.GetProperty("ResponseAsync")
            let task = pi.GetValue(callResult) :?> Task //Task<ResponseItemType)
            do! task
            let respType = typedefof<Task<_>>.MakeGenericType this.ResponseItemType
            let resultProperty = respType.GetProperty("Result")
            return resultProperty.GetValue(task)
        }

    member this.GetResponseHeaders(callResult: obj) =
        this.ResponseHeadersAsyncPropInfo.GetValue(callResult) :?> Task<Metadata>

type ClientStreamingCallResponse =
    { Headers: Metadata option
      Status: Status option
      Trailers: Metadata option
      WriteCompleted: bool
      CallResult: obj
      RequestStream: obj
      Response: obj
      CallInfo: ClientStreamingCallInfo }

    static member Empty() =
        { Headers = None
          Status = None
          Trailers = None
          WriteCompleted = false
          CallResult = Unchecked.defaultof<obj>
          Response = Unchecked.defaultof<obj>
          RequestStream = Unchecked.defaultof<obj>
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
    let private wrapResponse =
        let cache = Dictionary<Type, ClientStreamingCallInfo>()

        fun (methodInfo: MethodInfo) (resp: obj) (isError: bool) ->
            let args = methodInfo.ReturnType.GetGenericArguments()
            let requestItemType = args[0]
            let responseItemType = args[1]

            let clientStreamType =
                typedefof<AsyncClientStreamingCall<_, _>>
                    .MakeGenericType(requestItemType, responseItemType)

            let callInfo =
                let found, temp = cache.TryGetValue clientStreamType

                if found then
                    temp
                else
                    let requestStreamPropInfo = clientStreamType.GetProperty("RequestStream") //IClientStreamWriter<TRequest>
                    let requestStreamType = requestStreamPropInfo.GetValue(resp).GetType() //requestStreamPropInfo.PropertyType
                    let writeMethod = requestStreamType.GetMethod("WriteAsync", [| requestItemType |])
                    let completeMethod = requestStreamType.GetMethod("CompleteAsync")

                    let responseHeadersAsyncPropInfo =
                        clientStreamType.GetProperty("ResponseHeadersAsync")

                    let getStatusMethodInfo = clientStreamType.GetMethod("GetStatus")
                    let getTrailersMethodInfo = clientStreamType.GetMethod("GetTrailers")

                    let temp =
                        { ClientStreamItemType = requestItemType
                          ClientStreamType = clientStreamType
                          RequestItemType = requestItemType
                          ResponseItemType = responseItemType
                          GetStatusMethodInfo = getStatusMethodInfo
                          GetTrailersMethodInfo = getTrailersMethodInfo
                          WriteAsyncMethodInfo = writeMethod
                          CompleteAsyncMethodInfo = completeMethod
                          ResponseHeadersAsyncPropInfo = responseHeadersAsyncPropInfo
                          RequestStreamPropInfo = requestStreamPropInfo
                          Method = methodInfo}

                    cache[clientStreamType] <- temp
                    temp

            let requestStream = callInfo.RequestStreamPropInfo.GetValue(resp)

            { Headers = None
              Trailers = None
              Status = None
              WriteCompleted = false
              CallResult = resp
              Response = Unchecked.defaultof<obj>
              RequestStream = requestStream
              CallInfo = callInfo }

    let emitClientStreamResponse (resp: ClientStreamingCallResponse) = task {
        let isError = resp.Response :? Exception
         
        let wrapperType = ResponseUtils.emitClientStreamResponse resp.CallInfo.Method isError
        let wrapperInst = Activator.CreateInstance wrapperType
        let responsePi = wrapperType.GetProperty("Response")
        let response =
            if (isError) then
                let exc = resp.Response :?> Exception
                ErrorResponse(Error = exc.Message) |> box
            else
                resp.Response
                
        responsePi.SetValue(wrapperInst, response)
       
        let headerProp = wrapperType.GetProperty("Headers")
        headerProp.SetValue(wrapperInst, match resp.Headers with | Some m -> m | None -> Metadata())
        
        let statusProp = wrapperType.GetProperty("Status")       
        statusProp.SetValue(wrapperInst, match resp.Status with | Some s -> s | None -> Status(StatusCode.Unknown, "check the logs") )
        
        let trailersProp =wrapperType.GetProperty("Trailers")
        trailersProp.SetValue(wrapperInst, match resp.Trailers with | Some m -> m | None -> Metadata())
            
        return struct (wrapperType, wrapperInst)                            
       
        }
        
        
    let completeCall (resp: ClientStreamingCallResponse) =
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

    let getResponseHeader (resp: ClientStreamingCallResponse) =
        task {
            try
                let! meta = resp.CallInfo.GetResponseHeaders(resp.CallResult) //prop.GetValue(okayResp.CallResult) :?> Task<Metadata>
                return { resp with Headers = Some meta }
            with exc -> return resp
        }

    let completeWrite (resp: ClientStreamingCallResponse) =
        task {
            try
                do! (resp.CallInfo.CompleteAsyncMethodInfo.Invoke(resp.RequestStream, null) :?> Task)
                return { resp with WriteCompleted = true }
            with exc ->
                return { resp with WriteCompleted = true }
        }

    let getResponse (resp: ClientStreamingCallResponse) =
        task {
            try
                let! methodCallResponse = resp.CallInfo.GetResponse(resp.CallResult)
                return { resp with Response = methodCallResponse }
            with exc ->
                return { resp with Response = exc }
        }


    let write (resp: ClientStreamingCallResponse) (reqItem: obj) = task {
      do! (resp.CallInfo.WriteAsyncMethodInfo.Invoke(resp.RequestStream, [| reqItem |]) :?> Task)
     }
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
