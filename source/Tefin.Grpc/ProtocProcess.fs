namespace Tefin.Grpc.Compiler

open System
open System.Collections.Generic
open System.IO
//open Bym.Core.CS
open Tefin.Core
open Tefin.Core.Utils

module ProtocProcess =
  let private cleanupWorkingDirectory (io: IOs) (grpcRoot: string) (protosPath: string) =
    //Clean up old proto files in the working directory
    let existingFiles =
      //io.DirIO.G
      io.Dir.GetFiles(grpcRoot, "*.proto", SearchOption.TopDirectoryOnly)

    for f in existingFiles do
      io.File.Delete(f)

    //Clean up old C# source files
    for cs in io.Dir.GetFiles(protosPath, "*.cs", SearchOption.AllDirectories) do
      io.File.Delete(cs)

  let private getProtocArgs (io: IOs) (grpcRoot: string) (csharpPlugin: string) (protoFiles: string array) =
    let googlePath = Path.Combine(grpcRoot, "google", "protobuf")
    let sourceProtoPath = Path.GetDirectoryName(protoFiles[0])

    let args =
      $"--proto_path=\"{grpcRoot}\" --proto_path=\"{sourceProtoPath}\" --proto_path=\"{googlePath}\" --csharp_out=protos --csharp_opt=file_extension=.g.cs --plugin=protoc-gen-grpc={csharpPlugin} --grpc_out=protos "

    if (protoFiles.Length = 1) then
      $"{args} {Path.GetFileName(protoFiles[0])}"
    else
      let names = protoFiles |> Array.map (fun f -> Path.GetFileName(f))
      let files = String.Join(" ", names)
      $"{args} {files}"

  let extractExecutablesAndGoogleProtos
    (io: IOs)
    (grpcRootPath: string)
    (protocExe: string)
    (csharpPluginExe: string)
    =
    task {

      let toolsPath = Path.Combine(grpcRootPath, "tools")
      let protocPath = Path.Combine(toolsPath, "protoc")
      let zip = Path.Combine(grpcRootPath, "protoc.zip")

      if not (io.Dir.Exists(protocPath)) then
        do! io.File.WriteAllBytesAsync zip Tefin.Resources.Properties.Resources.protocZip
        io.File.ExtractZip zip toolsPath

      let protocFile, csharpPluginFile =
        if (isWindows ()) then
          let winPath = Path.Combine(toolsPath, "protoc", "windows_x64")
          ignore <| Directory.CreateDirectory winPath
          Path.Combine(winPath, protocExe), Path.Combine(winPath, csharpPluginExe)
        else if (isMac ()) then
          let macPath = Path.Combine(toolsPath, "protoc", "macosx_x64")
          ignore <| Directory.CreateDirectory macPath
          Path.Combine(macPath, protocExe), Path.Combine(macPath, csharpPluginExe)
        else
          let linuxPath = Path.Combine(toolsPath, "protoc", "linux_x64")
          ignore <| Directory.CreateDirectory linuxPath
          Path.Combine(linuxPath, protocExe), Path.Combine(linuxPath, csharpPluginExe)

      let p = Path.Combine(grpcRootPath, protocExe)

      if not (io.File.Exists p) then
        io.File.Copy(protocFile, p)

      let c = Path.Combine(grpcRootPath, csharpPluginExe)

      if not (io.File.Exists c) then
        io.File.Copy(csharpPluginFile, c)

      let googlePath = Path.Combine(grpcRootPath, "google", "protobuf")
      io.Dir.CreateDirectory(googlePath)

      let protoFiles =
        [| "_struct"
           "any"
           "api"
           "descriptor"
           "duration"
           "empty"
           "field_mask"
           "source_context"
           "timestamp"
           "type"
           "wrappers" |]

      let mgr = Tefin.Resources.Properties.Resources.ResourceManager

      for file in protoFiles do
        let bytes =
          mgr.GetObject(file, Tefin.Resources.Properties.Resources.Culture) :?> byte array

        let f = file.Replace("_", "")
        let target = Path.Combine(googlePath, $"{f}.proto")

        if not (io.File.Exists(target)) then
          do! io.File.WriteAllBytesAsync target bytes
    }

  let generateSourceFiles (io: IOs) (grpcRoot: string) (protosPath: string) (protosFiles: string array) =
    task {
      //        protoc.exe
      //          --proto_path = protos ????
      //          --csharp_out = protos
      //          --csharp_opt = file_extension =.g.cs
      //          --plugin = protoc - gen - grpc = grpc_csharp_plugin.exe
      //          --grpc_out = protos
      //          myFile.proto

      let csharpPlugin, protoc =
        if (isWindows ()) then
          "grpc_csharp_plugin.exe", "protoc.exe"
        else
          "grpc_csharp_plugin", "protoc"

      do! extractExecutablesAndGoogleProtos io grpcRoot protoc csharpPlugin
      io.Dir.SetCurrentDirectory grpcRoot
      cleanupWorkingDirectory io grpcRoot protosPath

      //Copy the protofiles to the working directory
      let workingProtoFiles = List<string>(protosFiles)

      //for protoFile in protosFiles do
      //    let tempProtosFile = Path.Combine(grpcRoot, Path.GetFileName protoFile)
      //    io.File.Copy(protoFile, tempProtosFile, true)
      //    workingProtoFiles.Add(tempProtosFile)

      //var googlePath = Path.Combine(grpcRoot, "google", "protobuf");
      //var args = $"--proto_path={googlePath} --csharp_out=protos --csharp_opt=file_extension=.g.cs --plugin=protoc-gen-grpc={csharpPlugin} --grpc_out=protos {Path.GetFileName(tempProtosFile)}";
      let args = getProtocArgs io grpcRoot csharpPlugin (workingProtoFiles.ToArray())

      let! files = Proc.run protoc args (fun () -> io.Dir.GetFiles(protosPath, "*.cs", SearchOption.TopDirectoryOnly))

      return files
    }
