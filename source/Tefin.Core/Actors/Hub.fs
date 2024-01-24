namespace Tefin.Core.Infra.Actors

open System
open System.Threading.Tasks
open Tefin.Core.Infra.Actors

module Hub =
    open System.Collections.Generic
    open Actor

    type IMsg =
        abstract member Id: string

    type IActor =
        abstract member ReqType: Type
        abstract member Actor: Actor.Actor<IMsg>

    type IHub =
        abstract member Actor: IActor
        abstract member GetActor: Type -> IActor
        abstract member Register: Type * IActor -> unit
        abstract member Publish: MessageType<IMsg> -> unit
        abstract member Subscribe: Action<'r> -> unit

        abstract member SubscribeTask: Func<'r, Task> -> unit

    type HubActor(actor) =

        interface IActor with
            member x.ReqType = typeof<MessageType<IMsg>>
            member x.Actor = actor

    let createReq r : MessageType<IMsg> =
        let req = { Req = r; ReqType = r.GetType() }
        Message(req)

    let createSys r : MessageType<IMsg> = Actor.System(r)

    let createHub () =
        let hubActor, sub =
            let registrations = Dictionary<Type, ResizeArray<IActor>>()

            let handleSystem (m: SystemType) =
                async {
                    for a in registrations do
                        let s = createSys m

                        for actor in a.Value do
                            actor.Actor.Post s

                    if (m = SystemType.Close) then
                        registrations.Clear()
                }

            let handleMsg (m: Req<IMsg>) =
                async {
                    let ok, agents = registrations.TryGetValue(m.ReqType)

                    if ok then
                        let s = createReq m.Req

                        for actor in agents do
                            actor.Actor.Post s

                }

            let temp = Actor.createOneWay handleSystem handleMsg
            temp, registrations

        let h2 = HubActor(hubActor)

        //object-expression that implements IHub
        { new IHub with
            member x.Actor = h2

            member x.GetActor(reqType) =
                let ok, actors = sub.TryGetValue reqType

                if ok then
                    actors[0]
                else
                    failwith $"actor not found for request type {reqType.Name}"

            member x.Register(reqType, childActor) =
                let ok, actors = sub.TryGetValue reqType

                if (not ok) then
                    sub.Add(reqType, ResizeArray([| childActor |]))
                else
                    sub[reqType].Add childActor

            member x.Publish(m) = hubActor.Post m

            member x.Subscribe(handler: Action<'r>) =
                let sysHandler (c: Actor.SystemType) = async { () }

                let msgHandler (req: Actor.Req<IMsg>) =
                    async {
                        let s: 'r = (box req.Req) :?> 'r
                        handler.Invoke(s)
                    }

                let actor = Actor.createOneWay<IMsg> sysHandler msgHandler
                let h2 = HubActor actor
                x.Register(typeof<'r>, h2)

            member x.SubscribeTask(handler: Func<'a, Task>) =
                let sysHandler (c: Actor.SystemType) = async { () }

                let msgHandler (req: Actor.Req<IMsg>) =
                    async {
                        let s: 'a = (box req.Req) :?> 'a
                        do! handler.Invoke(s) |> Async.AwaitTask
                    }

                let actor = Actor.createOneWay<IMsg> sysHandler msgHandler
                let h2 = HubActor actor
                x.Register(typeof<'a>, h2) }
