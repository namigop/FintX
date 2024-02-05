namespace Tefin.Core

open System.Collections.Generic
open System
open System.IO
open Tefin.Core.Infra.Actors
open Tefin.Core.Infra.Actors.Logging
open Tefin.Core.Interop
open Tefin.Core.Log

type IOResolver =
  {
    File : IFileIO
    Zip : IZipIO
    Dir : IDirIO
    Log : ILog
    MethodCall : IMethodCallIO
    CreateWriter : StreamWriter -> ITextWriter
  }
  // //abstract Register<'a> : (unit -> 'a) -> unit
  // //abstract Resolve<'a> : unit -> 'a
  // abstract File: IFileIO
  // abstract Zip : IZipIO
  // abstract Dir: IDirIO
  // abstract Log: ILog
  // abstract MethodCall: IMethodCallIO
  // abstract CreateWriter: StreamWriter -> ITextWriter

module Resolver =
  let private c: Dictionary<System.Type, unit -> obj> =
    Dictionary<Type, unit -> obj>()

  let private register<'a> (builder: unit -> 'a) =
    let t = typeof<'a>
    let b = fun () -> builder () :> obj
    c.TryAdd(t, b) |> ignore

  let private resolve<'a> () =
    let ok, f = c.TryGetValue(typeof<'a>)

    if ok then
      f () :?> 'a
    else
      failwith $"Unable to resolve {typeof<'a>.FullName}"

  let private wrappedLogger =
    let temp = LogActor.create () :> ILog

    { new ILog with
        member x.Info msg = temp.Info msg

        member x.Warn msg =
          temp.Warn msg
          GlobalHub.publish (MsgShowFooter.Warn msg)

        member x.Debug msg = temp.Debug msg

        member x.Error(msg: string) =
          temp.Error msg
          GlobalHub.publish (MsgShowFooter.Error msg)

        member x.Error(exc: Exception) =
          temp.Error exc
          GlobalHub.publish (MsgShowFooter.Error exc.Message) }

  let value =
    let logger = wrappedLogger
    {
      Zip =  Zip.zipIO
      File = File.fileIO
      Dir = Dir.dirIO
      Log = logger
      MethodCall = MethodCall.methodCallIo
      CreateWriter =  Writer.writerIO
    }

    // { new IOResolver with
    //     //member x.Register<'a> builder =  register<'a> builder
    //     //member x.Resolve<'a>() = resolve<'a>()
    //     
    //     member x.Zip = Zip.zipIO
    //     member x.File = File.fileIO
    //     member x.Dir = Dir.dirIO
    //     member x.Log = logger
    //     member x.MethodCall = MethodCall.methodCallIo
    //     member x.CreateWriter w = Writer.writerIO w }
