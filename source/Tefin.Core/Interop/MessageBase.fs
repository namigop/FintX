namespace Tefin.Core.Interop


open Tefin.Core
open Tefin.Core.Infra.Actors.Hub

[<AutoOpen>]
module MessageBase =

  //base message clas
  type MsgBase(id: string) =
    interface IMsg with
      member x.Id = id

[<AutoOpen>]
module MessageFooter =
  type MsgShowFooter(msg: string, color: string) =
    inherit MsgBase("")

    new() = MsgShowFooter("", "Transparent")
    member x.Message = msg
    member x.Color = color
    static member Info(msg) = MsgShowFooter(msg, Colors.info)
    static member Warn(msg) = MsgShowFooter(msg, Colors.warning)
    static member Error(msg) = MsgShowFooter(msg, Colors.error)
