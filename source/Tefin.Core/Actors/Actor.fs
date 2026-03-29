namespace Tefin.Core.Infra.Actors

open System
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Microsoft.FSharp.Control
open Tefin.Core

module Actor =

  type Req<'a> =
    { ReqType: Type
      //RespType : Type
      Req: 'a }

  type SystemType =
    | Stop = 0
    | Start = 1
    | Close = 2

  type MessageType<'a> =
    | System of SystemType
    | Message of Req<'a>

  type TwoWay<'a, 'b> = MessageType<'a> * AsyncReplyChannel<'b>

  type Actor<'a> =
    { Post: MessageType<'a> -> unit
      ReqType: Type }

  type Actor2<'a, 'b>(r: Type, b: MessageType<'a> -> 'b, dispose) =
    member val ReqType = r
    member val PostAndReply = b

    interface IDisposable with
      member x.Dispose() = dispose ()

  let createOneWay<'a> handleSys handleMsg =
    let proc =
      MailboxProcessor<MessageType<'a>>.Start(fun inbox ->
        let mutable run = true
         
        let rec loop () =          
          async {
            if run then              
              let! message = inbox.Receive()
             
              match message with
              | System systemType ->
                if systemType = SystemType.Stop then
                  run <- false
                  return ()
              
                let! result = handleSys systemType |> Async.Catch
                match result with
                | Choice1Of2() -> ()
                | Choice2Of2 exn -> Console.WriteLine(exn)

              | Message m ->
                let! result = handleMsg m |> Async.Catch
                match result with
                | Choice1Of2() -> ()
                | Choice2Of2 exn -> Console.WriteLine(exn)

              return! loop ()
          }
        loop ())

    //return record IActor<'a,'b>
    { Post = fun msg -> proc.Post(msg)
      ReqType = typeof<'a> }

  let createTwoWay<'a, 'b> handleSys handleMsg =
    let proc =
      MailboxProcessor<TwoWay<'a, Ret<'b>>>.Start(fun inbox ->
        let mutable run = true

        let rec loop () =
          async {
            if run then
              let! message, rc = inbox.Receive()

              match message with
              | System systemType ->
                if systemType = SystemType.Stop then
                  run <- false

                let! result = handleSys systemType |> Async.Catch
                match result with
                | Choice1Of2 r -> rc.Reply(Ret.Ok r)
                | Choice2Of2 exn -> rc.Reply(Ret.Error exn)

              | Message m ->
                let! result = handleMsg m |> Async.Catch
                match result with
                | Choice1Of2 r -> rc.Reply(Ret.Ok r)
                | Choice2Of2 exn -> rc.Reply(Ret.Error exn)

              do! loop ()
          }

        loop ())

    let dispose () =
      let stop = SystemType.Stop
      let stopMsg: MessageType<'a> = System stop
      let _ = proc.PostAndReply(fun rc -> stopMsg, rc)
      ()

    let m = fun msg -> proc.PostAndReply(fun rc -> msg, rc)
    let actor2 = new Actor2<'a, Ret<'b>>(typeof<'a>, m, dispose)
    actor2
