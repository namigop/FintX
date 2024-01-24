namespace Tefin.Core

open Tefin.Core.Infra.Actors
open Tefin.Core.Infra.Actors.Hub

type IMethodCallIO =
    abstract Publish: clientName: string * method: string * point: double -> unit

type MethodCallMessage(clientName: string, method: string, point: double) =
    member x.ClientName = clientName
    member x.Method = method
    member x.Point = point

    interface IMsg with
        member x.Id = $"{clientName}/{method}"

module MethodCall =
    let methodCallIo =
        { new IMethodCallIO with
            member x.Publish(clientName: string, method: string, point: double) =
                GlobalHub.publish (MethodCallMessage(clientName, method, point)) }
