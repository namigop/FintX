namespace Tefin.Core.Interop


open Tefin.Core
open Tefin.Core.Infra.Actors.Hub

[<AutoOpen>]
module Messages =


    //base message clas
    type MsgBase(id: string) =
        interface IMsg with
            member x.Id = id

    //Message published when a project is loaded
    type MsgProjectLoaded(proj: Project, folderPath: string) =
        inherit MsgBase($"Project@{folderPath}")
        member x.Project = proj
        member x.Path = folderPath

    type MsgClientUpdated(client: ClientGroup, folderPath: string, previousPath: string) =
        inherit MsgBase($"Client@{folderPath}")
        member x.Client = client
        member x.PreviousPath = previousPath
        member x.Path = folderPath

    type MsgShowFooter(msg: string, color: string) =
        inherit MsgBase("")

        new() = MsgShowFooter("", "Transparent")
        member x.Message = msg
        member x.Color = color
        static member Info(msg) = MsgShowFooter(msg, Colors.info)
        static member Warn(msg) = MsgShowFooter(msg, Colors.warning)
        static member Error(msg) = MsgShowFooter(msg, Colors.error)
