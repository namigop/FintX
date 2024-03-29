namespace Tefin.Grpc.Discovery

open System
open System.Collections.ObjectModel
open System.Threading.Tasks
open Tefin.Core
open Tefin.Core.Res
open Tefin.Grpc.Compiler

type ProtoDiscoverParameters =
  { ProtoFiles: string array
    CustomClientName: string
    Description: string
    Config: ReadOnlyDictionary<string, string> }

module ProtoDiscoveryClient =

  let private validateProtos (io: IOs) (protos: string array) =
    let invalidEntries =
      protos
      |> Array.filter (fun location -> not (location.EndsWith("proto") && io.File.Exists(location)))

    if invalidEntries.Length > 0 then
      Ret.Error(failwith $"{invalidEntries[0]} is invalid")
    else
      Ret.Ok protos

  let private generateSourceFiles (io: IOs) (discoParams: ProtoDiscoverParameters) (protos: string array) =
    task {
      let! files =
        ProtocProcess.generateSourceFiles io discoParams.Config["RootPath"] discoParams.Config["ProtosPath"] protos

      if (files.Length = 0) then
        let msg = String.Join("\r\n", discoParams.ProtoFiles)
        return Error(Exception($"Unable to generate client code for {msg}"))
      else
        return Ok files
    }

  let generateSource (io: IOs) (discoParams: ProtoDiscoverParameters) =
    task {
      let msg = String.Join("\r\n", discoParams.ProtoFiles)
      io.Log.Info($"Discovering : {discoParams.CustomClientName} @ {msg}")

      let! csFilesResult =
        (Ret.Ok discoParams.ProtoFiles)
        |> bind (fun protos -> validateProtos io protos)
        |> Task.FromResult
        |> mapTask (fun protos -> generateSourceFiles io discoParams protos)

      return csFilesResult
    }
