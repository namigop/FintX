namespace Tefin.Grpc

open System

//open Bym.Grpc.Core.GrpcServerReflection
open System.Collections.ObjectModel
open System.Reflection
open System.Text.RegularExpressions
open System.Threading.Tasks
open Grpc.Core
open Tefin.Core
open Tefin.Core.Res
open System.IO
open Tefin.Grpc.Discovery

type DiscoverParameters =
    { ProtoFiles: string array
      ReflectionUri: Uri }

type CompileParameters =
    { Name: string
      Description: string
      ServiceName: string
      ProtoFiles: string array
      CsFiles: string array
      ReflectionServiceUrl: string
      Config: ReadOnlyDictionary<string, string> }

type GrpcMethodType =
    | Unary = 0
    | ServerStreaming = 1
    | ClientStreaming = 2
    | Duplex = 3

module ServiceClient =

    let getGrpcMethodType (mi: MethodInfo) =
        let returnType = mi.ReturnType
        if (returnType.Name.StartsWith("AsyncDuplexStreaming")) then
            GrpcMethodType.Duplex
        else if (returnType.Name.StartsWith("AsyncClientStreaming")) then
            GrpcMethodType.ClientStreaming
        else if (returnType.Name.StartsWith("AsyncServerStreaming")) then
            GrpcMethodType.ServerStreaming
        else
            GrpcMethodType.Unary

    let findClientType (types: Type array) =
        let rec check (t: Type) =
            if not (t.BaseType = null) then
                let ok = t.BaseType.FullName = "Grpc.Core.ClientBase"
                if not ok then check t.BaseType else true
            else
                false
        //public Type ServiceClientType => this.GeneratedTypes.First(t => t.BaseType?.BaseType?.FullName == "Grpc.Core.ClientBase");
        types |> Array.tryFind (fun t -> check t)

    let findMethods (serviceClientType: Type) =
        let excluded = [| "WithHost"; "ToString"; "Equals"; "GetType"; "GetHashCode" |]
        let methods = serviceClientType.GetMethods() |> Array.filter (fun m -> not m.IsSpecialName)

        methods
        |> Array.filter (fun m -> not (Array.contains m.Name excluded))
        |> Array.filter (fun m ->
            let lastParam = m.GetParameters() |> Array.last
            not (lastParam.ParameterType = typeof<CallOptions>))

    let generateSourceFiles (io: IOResolver) (compileParams: CompileParameters) =
        task {
            let grpcParams = {compileParams with Config = GrpcPackage.grpcConfigValues }
            let locations = grpcParams.ProtoFiles
            let address = grpcParams.ReflectionServiceUrl

            if (locations.Length = 0) then
                let dp: ServerDiscoverParameters =
                    { Address = address
                      ServiceName = grpcParams.ServiceName
                      CustomClientName = grpcParams.Name
                      Description = grpcParams.Description
                      Config = grpcParams.Config }

                io.Log.Info $"Generating client code from {address}"
                let! cs = ServerReflectionDiscoveryClient.generateSource io dp
                return cs
            else
                let dp: ProtoDiscoverParameters =
                    { ProtoFiles = locations
                      CustomClientName = grpcParams.Name
                      Description = grpcParams.Description
                      Config = grpcParams.Config }

                io.Log.Info $"Generating client code from {locations}"
                let! csFiles = ProtoDiscoveryClient.generateSource io dp
                return csFiles

        }

    let getServices (io: IOResolver) (discoParams: DiscoverParameters) =
        task {
            let! services = GrpcReflectionClient.getServices io discoParams.ReflectionUri.AbsoluteUri
            return services
        }

    let compile (io: IOResolver) (sourceFiles:string array) (compileParams: CompileParameters) =
        task {
            let grpcParams = {compileParams with Config = GrpcPackage.grpcConfigValues
                                                 CsFiles =  sourceFiles }
            
            let sourceFiles = grpcParams.CsFiles |> Array.map (fun f -> Path.GetFileName(f))
            let msg = String.Join(", ", sourceFiles)
            io.Log.Info($"Compiling : {msg}")
            let tempFile = Path.GetTempFileName()
            io.File.Delete(tempFile)

            let grpcClientFileOpt =
                grpcParams.CsFiles |> Array.tryFind (fun f -> f.EndsWith("Grpc.cs"))

            match grpcClientFileOpt with
            | None -> return Ret.Error(Exception("Unable to generate the source code of the service client"))
            | Some grpcClientFile ->
                let rootPath = grpcParams.Config["RootPath"]
                let name = Path.GetFileNameWithoutExtension(grpcClientFile)
                let assemblyName = $"{name}_{Path.GetFileNameWithoutExtension(tempFile)}"

                let assemblyFile =
                    Path.Combine(rootPath, "clients", assemblyName, $"{assemblyName}.dll")

                let! compileOutput = Task.Run(fun () -> ClientCompiler.compile io assemblyFile grpcParams.CsFiles)
                return compileOutput
        }
        
    let discover (io:IOResolver) (discoParams : DiscoverParameters) = task {
        if (discoParams.ProtoFiles.Length > 0) then
            let regex = @"service\s+(?<ServiceName>\w+)\s+";
            let lines = io.File.ReadAllLines discoParams.ProtoFiles[0]
            let services =  
                lines 
                |> Array.map (fun l -> Regex.Match(l, regex)) 
                |> Array.filter (fun g -> g.Success)
                |> Array.map (fun m -> m.Groups["ServiceName"].Value)
            
            return (Ret.Ok services)

         else
            let! services = GrpcReflectionClient.getServices io discoParams.ReflectionUri.AbsoluteUri
            return services
    }

  
 