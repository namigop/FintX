namespace Tefin.Core

open System.Collections.Generic
open System
open System.IO
open Tefin.Core.Infra.Actors
open Tefin.Core.Infra.Actors.Logging
open Tefin.Core.Interop
open Tefin.Core.Log

type IOs =
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
