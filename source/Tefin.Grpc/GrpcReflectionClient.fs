namespace Tefin.Grpc

open System
open System.Collections.Generic
open System.Threading
open System.Linq
open System.IO
open System.Threading.Tasks
open Google.Protobuf.Reflection

open Grpc.Net.Client
open Grpc.Reflection.V1Alpha

open Tefin.Core
open Tefin.Core.Res

module GrpcReflectionClient =
    let private Indent = "  "
    let private NoIndent = ""

    let createReflectionClient (address: string) =
        exec (fun () ->
            if address.StartsWith("http://") then
                // This switch must be set before creating the GrpcChannel/HttpClient.
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true)

            let channel = GrpcChannel.ForAddress(address)
            let client = ServerReflection.ServerReflectionClient(channel)
            client)

    let private writeMethodDescription (method: MethodDescriptor) (writer: ITextWriter) (indentation: string) =
        task {
            do! writer.WriteAsync($"{indentation} rpc {method.Name}(")

            if method.IsClientStreaming then
                do! writer.WriteAsync("stream ")

            do! writer.WriteAsync($"{method.InputType.FullName}) returns (")

            if method.IsServerStreaming then
                do! writer.WriteAsync("stream ")

            do! writer.WriteLineAsync($"{method.OutputType.FullName});")
        }

    let private writeServiceDescriptor (service: ServiceDescriptor) (writer: ITextWriter) (indentation: string) =
        task {
            do! writer.WriteLineAsync($"service {service.Name} {{")

            for method in service.Methods do
                do! writeMethodDescription method writer (indentation + Indent)

            do! writer.WriteLineAsync($"{indentation}}}")
        }

    let private writeFieldDescriptor (field: FieldDescriptor) (writer: ITextWriter) (indentation: string) =
        task {
            do! writer.WriteAsync(indentation)

            if field.IsRepeated then
                do! writer.WriteAsync("repeated ")

            match field.FieldType with
            | FieldType.Enum -> do! writer.WriteAsync(field.EnumType.Name)
            | FieldType.Message -> do! writer.WriteAsync(field.MessageType.FullName)
            | FieldType.Group -> ()
            | _ -> do! writer.WriteAsync(field.FieldType.ToString().ToLowerInvariant())

            do! writer.WriteLineAsync($" {field.Name} = {field.FieldNumber};")
        }

    let private writeOneOfDescriptor (oneof: OneofDescriptor) (writer: ITextWriter) (indentation: string) =
        task {
            do! writer.WriteLineAsync($"{indentation}oneof {oneof.Name} {{")

            for field in oneof.Fields do
                do! writeFieldDescriptor field writer (indentation + Indent)

            do! writer.WriteLineAsync($"{indentation}}}")
        }

    let rec private writeMessageDescriptor (message: MessageDescriptor) (writer: ITextWriter) (indentation: string) =
        task {
            do! writer.WriteAsync(indentation)
            do! writer.WriteLineAsync($"message {message.Name} {{")

            for nestedType in message.NestedTypes do
                do! writeMessageDescriptor nestedType writer (indentation + Indent)

            for field in
                message.Fields.InDeclarationOrder()
                |> Seq.filter (fun f -> f.ContainingOneof = null) do
                do! writeFieldDescriptor field writer (indentation + Indent)

            for oneof in message.Oneofs do
                do! writeOneOfDescriptor oneof writer (indentation + Indent)

            do! writer.WriteLineAsync($"{indentation}}}")
        }

    let private writeFileDescriptor (descriptor: FileDescriptor) (writer: ITextWriter) =
        task {
            // Syntax
            do! writer.WriteLineAsync("syntax = \"proto3\";")

            // Dependencies
            for dependency in descriptor.Dependencies do
                do! writer.WriteLineAsync($"import \"{dependency.Name}\";")

            // Package
            do! writer.WriteLineAsync($"package {descriptor.Package};")

            // Empty line
            do! writer.WriteLineAsync()

            // Messages
            for message in descriptor.MessageTypes do
                do! writeMessageDescriptor message writer NoIndent
                do! writer.WriteLineAsync()

            // Messages
            for service in descriptor.Services do
                do! writeServiceDescriptor service writer NoIndent
                do! writer.WriteLineAsync()
        }

    let private getDescriptors (service: string) (client: ServerReflection.ServerReflectionClient) =
        execTask (fun () ->
            task {
                let stream = client.ServerReflectionInfo()
                do! stream.RequestStream.WriteAsync(ServerReflectionRequest(FileContainingSymbol = service))
                let! _ = stream.ResponseStream.MoveNext(CancellationToken.None)

                let protos =
                    stream.ResponseStream.Current.FileDescriptorResponse.FileDescriptorProto.Reverse()

                let descriptors = FileDescriptor.BuildFromByteStrings(protos)
                do! stream.RequestStream.CompleteAsync()
                return descriptors
            })

    let private isWellKnownType (descriptor: FileDescriptor) =
        descriptor.Name.StartsWith("google/protobuf/")
        && descriptor.Package.Equals("google.protobuf")

    let private getProtos (io: IOResolver) (path: string) (descriptors: IReadOnlyList<FileDescriptor>) =
        task {
            let protos = List<string>()

            for descriptor in descriptors do
                if not (isWellKnownType descriptor) then
                    let file = Path.Join(path, Path.GetFileName(descriptor.Name))
                    let writer = io.File.CreateText(file) |> io.CreateWriter
                    do! writeFileDescriptor descriptor writer
                    do! writer.DisposeAsync()
                    protos.Add(file)

            if protos.Any() then
                return Ret.Ok protos
            else
                return Ret.Error(Exception("Empty protos"))
        }

    let private getAddress (address2: string) =
        if (not <| address2.StartsWith("http://") && not <| address2.StartsWith("https://")) then
            $"https://{address2}"
        else
            address2

    let createProtoFile (io: IOResolver) (address2: string) (service: string) (path: string) =
        task {
            let address = getAddress address2

            let! protos =
                Task.FromResult(createReflectionClient address)
                |> mapTask (getDescriptors service)
                |> mapTask (getProtos io path)

            return protos
        }

    let getServices (io: IOResolver) (address2: string) =
        task {
            let address = getAddress address2

            let! services =
                createReflectionClient (address)
                |> map (fun c -> c.ServerReflectionInfo())
                |> Task.FromResult
                |> mapTask (fun stream ->
                    task {
                        do! stream.RequestStream.WriteAsync(ServerReflectionRequest(ListServices = "ls"))
                        let! _ = stream.ResponseStream.MoveNext(CancellationToken.None)

                        let services =
                            stream.ResponseStream.Current.ListServicesResponse.Service
                            |> Seq.map (fun t -> t.Name)
                            |> Seq.toArray

                        do! stream.RequestStream.CompleteAsync()
                        io.Log.Info("Found services: " + String.Join(",", services))
                        return Ret.Ok services
                    })

            return services
        }
