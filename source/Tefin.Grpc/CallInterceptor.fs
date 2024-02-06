namespace Tefin.Grpc.Custom

open System.Diagnostics
open System.Threading.Tasks
open Grpc.Core
open Grpc.Core.Interceptors
open Tefin.Core
open System

type CallInterceptor(clientName: string, io: IOs, onErr: Exception -> unit) =
  inherit Interceptor()

  let onSuccess (name: string) retVal (ts: TimeSpan) =
    io.Log.Info $"Call to {name} done. Elapsed {ts.TotalMilliseconds} msec"

  let onError (name: string) (exc: Exception) (ts: TimeSpan) =
    onErr exc
    io.Log.Error $"Call to {name} failed. Elapsed {ts.TotalMilliseconds} msec"
    io.Log.Error $"{exc}"

  let rec tryExec (count: int) (doThis: unit -> 'a) =
    try
      doThis ()
    with exc ->
      if (count = 0) then
        tryExec (count + 1) doThis //retry once
      else
        raise exc

  let rec tryExecAsync (count: int) (doThis: unit -> Task<'a>) =
    task {
      try
        return! doThis ()
      with exc ->
        if (count = 0) then
          return! tryExec (count + 1) doThis //retry once
        else
          raise exc
          return Unchecked.defaultof<'a> //will this be reached??
    }

  let getWrappedResponseHeaders_ClientStream (clientName: string) (resp: AsyncClientStreamingCall<'TReq, 'TResp>) =
    task {
      let name = "ResponseHeadersAsync"
      let onSuccessOpt = Some(onSuccess name)
      let onErrorOpt = Some(onError name)
      let! meta, ts = TimeIt.runTaskWithReturnValue (fun () -> resp.ResponseHeadersAsync) onSuccessOpt onErrorOpt
      return meta
    //let! meta = resp.ResponseHeadersAsync
    //return meta
    }

  let getWrappedResponseHeaders_DuplexStream (clientName: string) (resp: AsyncDuplexStreamingCall<'TReq, 'TResp>) =
    task {
      let name = "ResponseHeadersAsync"
      let onSuccessOpt = Some(onSuccess name)
      let onErrorOpt = Some(onError name)
      let! meta, ts = TimeIt.runTaskWithReturnValue (fun () -> resp.ResponseHeadersAsync) onSuccessOpt onErrorOpt
      return meta
    }

  let getWrappedResponseHeaders_ServerStream (clientName: string) (resp: AsyncServerStreamingCall<'TResp>) =
    task {
      let name = "ResponseHeadersAsync"
      let onSuccessOpt = Some(onSuccess name)
      let onErrorOpt = Some(onError name)
      let! meta, ts = TimeIt.runTaskWithReturnValue (fun () -> resp.ResponseHeadersAsync) onSuccessOpt onErrorOpt
      return meta
    }

  let getWrappedStatus_ServerStream (service: string) (method: string) (resp: AsyncServerStreamingCall<'TResp>) =
    let name = "GetStatus"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (status, ts) =
      TimeIt.runActionWithReturnValue resp.GetStatus onSuccessOpt onErrorOpt

    status

  let getWrappedStatus_ClientStream (service: string) (method: string) (resp: AsyncClientStreamingCall<'TReq, 'TResp>) =
    let name = "GetStatus"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (status, ts) =
      TimeIt.runActionWithReturnValue resp.GetStatus onSuccessOpt onErrorOpt

    status

  let getWrappedStatus_DuplexStream (service: string) (method: string) (resp: AsyncDuplexStreamingCall<'TReq, 'TResp>) =

    let name = "GetStatus"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (status, ts) =
      TimeIt.runActionWithReturnValue resp.GetStatus onSuccessOpt onErrorOpt

    status


  let getWrappedTrailer_ServerStream (service: string) (method: string) (resp: AsyncServerStreamingCall<'TResp>) =
    let name = "GetTrailers"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (meta, ts) =
      TimeIt.runActionWithReturnValue resp.GetTrailers onSuccessOpt onErrorOpt

    meta

  let getWrappedTrailer_ClientStream
    (service: string)
    (method: string)
    (resp: AsyncClientStreamingCall<'TReq, 'TResp>)
    =
    let name = "GetTrailers"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (meta, ts) =
      TimeIt.runActionWithReturnValue resp.GetTrailers onSuccessOpt onErrorOpt

    meta

  let getWrappedTrailer_DuplexStream
    (service: string)
    (method: string)
    (resp: AsyncDuplexStreamingCall<'TReq, 'TResp>)
    =
    let name = "GetTrailers"
    let onSuccessOpt = Some(onSuccess name)
    let onErrorOpt = Some(onError name)

    let struct (meta, ts) =
      TimeIt.runActionWithReturnValue resp.GetTrailers onSuccessOpt onErrorOpt

    meta

  let getWrappedResponse_ClientStream
    (clientName: string)
    (method: string)
    (resp: AsyncClientStreamingCall<'TReq, 'TResp>)
    =
    task {
      let name = "ResponseAsync"
      let onSuccessOpt = Some(onSuccess name)
      let onErrorOpt = Some(onError name)
      let! resp, ts = TimeIt.runTaskWithReturnValue (fun () -> resp.ResponseAsync) onSuccessOpt onErrorOpt
      // let! resp = resp.ResponseAsync
      return resp
    }

  member x.ClientName = clientName

  override x.AsyncClientStreamingCall<'TReq, 'TResp when 'TReq: not struct and 'TResp: not struct>
    (
      context: ClientInterceptorContext<'TReq, 'TResp>,
      continuation: Interceptor.AsyncClientStreamingCallContinuation<'TReq, 'TResp>
    ) : AsyncClientStreamingCall<'TReq, 'TResp> =

    let call = continuation.Invoke(context)
    let method = context.Method.Name
    let getRespHeaderTask = getWrappedResponseHeaders_ClientStream clientName call

    let getStatus =
      fun () ->
        fun () -> getWrappedStatus_ClientStream clientName method call
        |> tryExec 0

    let getTrailer =
      fun () ->
        fun () -> getWrappedTrailer_ClientStream clientName method call
        |> tryExec 0

    let getResponseTask =
      fun () -> getWrappedResponse_ClientStream clientName method call
      |> tryExecAsync 0

    let dispose = call.Dispose

    let writer = TimedClientStreamWriter.create io call.RequestStream clientName method

    let newCall =
      new AsyncClientStreamingCall<'TReq, 'TResp>(
        writer,
        getResponseTask,
        getRespHeaderTask,
        getStatus,
        getTrailer,
        dispose
      )

    newCall

  override x.AsyncDuplexStreamingCall<'TReq, 'TResp when 'TReq: not struct and 'TResp: not struct>
    (
      context: ClientInterceptorContext<'TReq, 'TResp>,
      continuation: Interceptor.AsyncDuplexStreamingCallContinuation<'TReq, 'TResp>
    ) : AsyncDuplexStreamingCall<'TReq, 'TResp> =

    let call = continuation.Invoke(context)
    let method = context.Method.Name
    let getRespHeaderTask = getWrappedResponseHeaders_DuplexStream clientName call

    let getStatus =
      fun () ->
        fun () -> getWrappedStatus_DuplexStream clientName method call
        |> tryExec 0

    let getTrailer =
      fun () ->
        fun () -> getWrappedTrailer_DuplexStream clientName method call
        |> tryExec 0

    //let getResponseTask = getWrappedResponse_DuplexStream clientName method call
    let dispose = call.Dispose

    let writer = TimedClientStreamWriter.create io call.RequestStream clientName method
    let reader = TimedAsyncStreamReader.create io call.ResponseStream clientName method

    let newCall =
      new AsyncDuplexStreamingCall<'TReq, 'TResp>(writer, reader, getRespHeaderTask, getStatus, getTrailer, dispose)

    newCall

  override x.AsyncServerStreamingCall<'TReq, 'TResp when 'TReq: not struct and 'TResp: not struct>
    (
      request: 'TReq,
      context: ClientInterceptorContext<'TReq, 'TResp>,
      continuation: Interceptor.AsyncServerStreamingCallContinuation<'TReq, 'TResp>
    ) : AsyncServerStreamingCall<'TResp> =

    let call = continuation.Invoke(request, context)
    let method = context.Method.Name
    let getRespHeaderTask = getWrappedResponseHeaders_ServerStream clientName call

    let getStatus =
      fun () ->
        fun () -> getWrappedStatus_ServerStream clientName method call
        |> tryExec 0

    let getTrailer =
      fun () ->
        fun () -> getWrappedTrailer_ServerStream clientName method call
        |> tryExec 0

    let dispose = call.Dispose

    let reader = TimedAsyncStreamReader.create io call.ResponseStream clientName method

    let newCall =
      new AsyncServerStreamingCall<'TResp>(reader, getRespHeaderTask, getStatus, getTrailer, dispose)

    newCall

  override x.AsyncUnaryCall<'TReq, 'TResp when 'TReq: not struct and 'TResp: not struct>
    (
      request: 'TReq,
      context: ClientInterceptorContext<'TReq, 'TResp>,
      continuation: Interceptor.AsyncUnaryCallContinuation<'TReq, 'TResp>
    ) : AsyncUnaryCall<'TResp> =

    let sw = Stopwatch.StartNew()

    let method = $"{context.Method.Name}Async"
    let call = continuation.Invoke(request, context)

    call
      .GetAwaiter()
      .OnCompleted(fun () ->
        sw.Stop()
        let status = call.GetStatus()

        if (status.StatusCode = StatusCode.OK) then
          io.MethodCall.Publish(clientName, method, sw.Elapsed.TotalMilliseconds)
          onSuccess method "" sw.Elapsed
        else
          let rpc = RpcException(status)
          onError method rpc sw.Elapsed)

    call

  override x.BlockingUnaryCall<'TReq, 'TResp when 'TReq: not struct and 'TResp: not struct>
    (
      request: 'TReq,
      context: ClientInterceptorContext<'TReq, 'TResp>,
      continuation: Interceptor.BlockingUnaryCallContinuation<'TReq, 'TResp>
    ) =
    let method = context.Method.Name

    let onSuccessOpt = Some(onSuccess method)
    let onErrorOpt = Some(onError method)

    let struct (resp, ts) =
      TimeIt.runActionWithReturnValue (fun () -> continuation.Invoke(request, context)) onSuccessOpt onErrorOpt

    io.MethodCall.Publish(clientName, method, ts.TotalMilliseconds)
    resp
