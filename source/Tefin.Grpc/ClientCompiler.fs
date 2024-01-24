namespace Tefin.Grpc

open System
open System.IO
open Microsoft.CodeAnalysis
open Tefin.Core
open Tefin.Core.Build
open Tefin.Core.Res

module ClientCompiler =

    let private getGrpcReferencedFiles (io: IOResolver) =
        let exepath = AppContext.BaseDirectory // Path.GetDirectoryName(Environment.ProcessPath); //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        let grpcFiles =
            io.Dir.GetFiles(exepath, "Grpc.*.dll")
            |> Array.append [| Path.Combine(exepath, "Google.Protobuf.dll") |]

        if (grpcFiles.Length > 1) then
            Ret.Ok grpcFiles
        else
            Ret.Error(failwith "Missing Grpc.* dll references")

    let compile (io: IOResolver) (assemblyFile: string) (sourceFiles: string array) =
        getGrpcReferencedFiles io
        |> map (fun grpcDlls ->
            let cIn: CompileInput =
                { ModuleFile = assemblyFile
                  SourceFiles = sourceFiles
                  ReferencedFiles = grpcDlls
                  TargetOutput = OutputKind.DynamicallyLinkedLibrary
                  AdditionalReferences = grpcDlls |> Array.map (fun f -> MetadataReference.CreateFromFile f) }

            let output = ClientCompiler.compile io cIn

            if (output.Success) then
                output
            else
                let err = String.Join(Environment.NewLine, output.CompilationErrors)
                io.Log.Error err
                failwith err)
