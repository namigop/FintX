namespace Tefin.Core.Infra.Actors.Logging

open System
open Tefin.Core.Infra.Actors.Actor
open Tefin.Core.Infra.Actors.Hub
open Tefin.Core.Log

type Content =
  | Info of string
  | Warn of string
  | Debug of string
  | Error of string
  | Exc of Exception

type Req(c: Content, id: string option) =
  member x.Content = c

  interface IMsg with
    member x.Id =
      match id with
      | Some x -> x
      | None ->
        let mutable f = Guid.NewGuid().ToString()
#if Debug
        let frames = StackTrace().GetFrames()

        f <-
          frames
          |> Array.map (fun a -> a.GetMethod().Name)
          |> fun methods -> string.Join(".", methods)
#endif
        f

module LogActor =

  type T(actor: Actor<IMsg>) =
    interface ILog with
      member x.Info msg =
        let req = Req(Info msg, None) |> createReq
        actor.Post req

      member x.Warn msg =
        let req = Req(Warn msg, None) |> createReq
        actor.Post req

      member x.Debug msg =
        let req = Req(Debug msg, None) |> createReq
        actor.Post req

      member x.Error(msg: string) =
        let req = Req(Error msg, None) |> createReq
        actor.Post req

      member x.Error exc =
        let req = Req(Exc exc, None) |> createReq
        actor.Post req

  let private handleSys (sys: SystemType) (state: string) = async { return state, sys }

  let private handleMessage (msg: Req<IMsg>) (state: string) =
    async {
      let req = msg.Req :?> Req

      match (req.Content) with
      | Info msg -> return logInfo msg
      | Warn msg -> return logWarn msg
      | Debug msg -> return logDebug msg
      | Error msg -> return logError msg
      | Exc err -> return logExc err
    }

  let create () =
    let sysHandler req =
      async {
        let! _, _ = handleSys req ("")
        return ()
      }

    let msgHandler req =
      async { return! handleMessage req ("") }

    let actor = createOneWay sysHandler msgHandler
    T(actor)
