namespace Tefin.Grpc.Compiler

open System
open System.Collections.Generic
open System.Text.RegularExpressions
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

  let private getProtocArgs (io: IOs) (grpcRoot: string) (csharpPlugin: string) (protoFile: string) =
    let googlePath = Path.Combine(grpcRoot, "google", "protobuf")
    let sourceProtoPath = Path.GetDirectoryName(protoFile)

    // let args =
    //   $"--proto_path=\"{grpcRoot}\" --proto_path=\"{sourceProtoPath}\" --proto_path=\"{googlePath}\" --csharp_out=protos --csharp_opt=file_extension=.g.cs --plugin=protoc-gen-grpc={csharpPlugin} --grpc_out=protos "

    let oneLevelup = Path.GetDirectoryName(sourceProtoPath);
    let args =
      $"--proto_path=\"{grpcRoot}\" --proto_path=\"{sourceProtoPath}\"   --proto_path=\"{oneLevelup}\" --csharp_out=protos --csharp_opt=file_extension=.g.cs --plugin=protoc-gen-grpc={csharpPlugin} --grpc_out=protos "

    $"{args} \"{protoFile}\""
    // if (protoFiles.Length = 1) then
    //   //$"{args} {Path.GetFileName(protoFiles[0])}"
    //   $"{args} \"{protoFiles[0]}\""
    // else
    //   let names = protoFiles |> Array.map (fun f -> Path.GetFileName(f))
    //   let files = String.Join(" ", names)
    //   $"{args} {files}"

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

      let findImports (protoFile:string) =
        let lines = io.File.ReadAllLines protoFile
        lines
        |> Array.filter (fun line -> line.Trim().StartsWith("import"))
        |> Array.map (fun line -> line.Substring(line.IndexOf("\"")).Replace("\"", "").Replace(";", ""))
        |> Array.filter (fun line -> not <| line.StartsWith("google/protobuf"))
        
      let rec findFile (basePath:string) (filePart:string) =
        let file = Path.Combine(basePath, filePart)
        if (io.File.Exists file) then
          file        
        else
          //Move up one level because the proto files created by Visual studio do not have the
          //correct import path For example, if you want to import another protobuf that is in
          //same "Protos" folder you will need to write
          //
          //    import "Protos/other.proto".
          //
          // For non-.NET projects that will just be simply
          //
          //    import "other.proto"
          
          findFile (Path.GetDirectoryName basePath) filePart
          
      let warningRegex = new Regex(@"warning:\s+.*")         
      let isProtocError (line:string) = not (warningRegex.IsMatch(line))         
          
      let rec generateFor (protoFile:string) (csFiles:ResizeArray<string>) =
        task {
          let args = getProtocArgs io grpcRoot csharpPlugin protoFile
          let! files = Proc.run protoc args (fun () -> io.Dir.GetFiles(protosPath, "*.cs", SearchOption.TopDirectoryOnly)) isProtocError
          csFiles.AddRange files
          let basePath = Path.GetDirectoryName protoFile
          let imports = findImports protoFile        
          let importFiles = imports |> Array.map (fun p -> findFile basePath p)
          for i in importFiles do
            do! generateFor i csFiles
         }
         
      let csFiles = ResizeArray<string>()
      do! generateFor protosFiles[0] csFiles

      let generatedCsFiles = csFiles |> Seq.distinct |> Seq.toArray

      if (generatedCsFiles.Length > 0) then
        //If we have generated new *.cs files, delete any dll lying around
        let dlls = io.Dir.GetFiles(System.IO.Path.GetDirectoryName(generatedCsFiles[0]), "*.dll", System.IO.SearchOption.TopDirectoryOnly)
        for dll in dlls do
          io.File.Delete dll

      return generatedCsFiles
    }
