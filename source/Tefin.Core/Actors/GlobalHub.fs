namespace Tefin.Core.Infra.Actors

module GlobalHub =

    open System.Threading.Tasks
    open System

    let private hub = Hub.createHub ()

    let publish req =
        let msg = Hub.createReq req
        hub.Publish msg

    let register (reqType, agent) = hub.Register(reqType, agent)

    let register2 (actor) = hub.Register(actor)

    let subscribe (handler: Action<'r>) = hub.Subscribe(handler)
    let subscribeTask (handler: Func<'a, Task>) =  hub.SubscribeTask(handler)

    let getActor reqType = hub.GetActor reqType