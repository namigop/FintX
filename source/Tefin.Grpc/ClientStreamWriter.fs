namespace Tefin.Grpc.Custom

open System
open Grpc.Core
open System.Threading.Tasks
open Tefin.Core

type IClientStreamActions<'T> =
  abstract member OnCompleteAsync: IClientStreamWriter<'T> -> Task
  abstract member WriteAsync: IClientStreamWriter<'T> -> 'T -> Task

type ClientStreamWriter<'T>(writer: IClientStreamWriter<'T>, actions: IClientStreamActions<'T>) =
  member this.CompleteAsync() = actions.OnCompleteAsync writer
  member this.WriteAsync(message) = actions.WriteAsync writer message

  interface IClientStreamWriter<'T> with

    member this.WriteOptions
      with get () = writer.WriteOptions
      and set v = writer.WriteOptions <- v

    member this.CompleteAsync() = this.CompleteAsync()
    member this.WriteAsync(message) = this.WriteAsync message

//TODO: module PerfTrackingClientStreamWriter
module TimedClientStreamWriter =
  let private createAction<'T> (io: IOs) (clientName: string) (method: string) =
    let onSuccess (name: string) (ts: TimeSpan) =
      io.Log.Info $"Call to {name} done. Elapsed {ts.TotalMilliseconds} msec"
      io.MethodCall.Publish(clientName, method, ts.TotalMilliseconds)

    let onError (name: string) (exc: Exception) (ts: TimeSpan) =
      io.Log.Error $"Call to {name} failed. Elapsed {ts.TotalMilliseconds} msec"
      io.Log.Error $"{exc}"

    let onComplete (writer: IClientStreamWriter<'T>) =
      task {
        let name = "ClientStreamWriter.Complete"
        let onSuccessOpt = Some(onSuccess name)
        let onErrorOpt = Some(onError name)
        let! ts = TimeIt.runTask writer.CompleteAsync onSuccessOpt onErrorOpt
        ()
      }

    let onWrite (writer: IClientStreamWriter<'T>) msg =
      task {
        let name = "ClientStreamWriter.Write"
        let onSuccessOpt = Some(onSuccess name)
        let onErrorOpt = Some(onError name)
        let! ts = TimeIt.runTask (fun () -> writer.WriteAsync msg) onSuccessOpt onErrorOpt
        ()
      }

    { new IClientStreamActions<'T> with
        member x.OnCompleteAsync(writer) = task { do! (onComplete writer) }
        member x.WriteAsync writer msg = task { do! (onWrite writer msg) } }

  let create<'T> (io: IOs) (writer: IClientStreamWriter<'T>) (clientName: string) (method: string) =
    let actions = createAction io clientName method
    new ClientStreamWriter<'T>(writer, actions)
