namespace Tefin.Grpc

open System
open System.Reflection
open System.Runtime.InteropServices
open Google.Protobuf
open Grpc.Core
open Tefin.Core
open Tefin.Core.Reflection

module GrpcTypeBuilder =
    let private buildMeta (createInstance: bool) =
        if (createInstance) then
            let meta = new Metadata()
            
            meta.Add("client", $"{Utils.appName}({RuntimeInformation.FrameworkDescription})/@{Environment.MachineName}")
            struct (true, box meta)
        else
            struct (false, Unchecked.defaultof<obj>)

    let private buildMetaEntry (createInstance: bool) =
        if (createInstance) then
            let meta = Metadata.Entry("key", "value")
            struct (true, box meta)
        else
            struct (false, Unchecked.defaultof<obj>)

    let private buildWriteOptions (createInstance: bool) =
        if (createInstance) then
            struct (true, box WriteOptions.Default)
        else
            struct (false, Unchecked.defaultof<obj>)

    let private buildCallOptions (createInstance: bool) =
        if (createInstance) then
            struct (true, CallOptions() |> box)
        else
            struct (false, Unchecked.defaultof<obj>)

    let private buildTimestamp (createInstance: bool) =
        if (createInstance) then
            let now = DateTime.Now.AddDays(1).ToUniversalTime();
            let t = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now)
            struct (true,  box t )
        else
            struct (false, Unchecked.defaultof<obj>)

    let buildRepeatedField (thisType: Type) (parentInstanceOpt: obj option) depth =
        match parentInstanceOpt with
        | Some parentInstance ->
            match (TypeHelper.getListItemType thisType) with
            | Some itemType ->
                let addMethod = thisType.GetMethod("Add", [| itemType |])
                let struct (ok, itemInstance) = TypeBuilder.getDefault itemType true None depth
                let rAdd = addMethod.Invoke(parentInstance, [| itemInstance |]) 
                struct (true, parentInstance)
            | None -> struct (false, Unchecked.defaultof<obj>)
        | None -> struct (false, Unchecked.defaultof<obj>)
        

    let getDefault (thisType: System.Type) (createInstance: bool) (parentInstanceOpt: obj option) depth =
        if (thisType = typeof<Nullable<DateTime>>) then
            struct (true, DateTime.UtcNow.AddDays(1) |> box)
        elif (thisType = typeof<ByteString>) then
            struct (true, ByteString.CopyFrom("", System.Text.Encoding.UTF8) |> box)
        elif (thisType.FullName.StartsWith("Google.Protobuf.Collections.MapField`2")) then
            DictionaryType.getDefault thisType createInstance parentInstanceOpt depth
        elif (thisType.FullName.StartsWith("Google.Protobuf.Collections.RepeatedField`1")) then
            buildRepeatedField thisType parentInstanceOpt depth
        elif (thisType = typeof<Metadata>) then
            buildMeta createInstance
        elif (thisType = typeof<Metadata.Entry>) then
            buildMetaEntry createInstance
        elif (thisType = typeof<WriteOptions>) then
            buildWriteOptions createInstance
        elif (thisType = typeof<CallOptions>) then
            buildCallOptions createInstance
        elif (thisType = typeof<Google.Protobuf.WellKnownTypes.Timestamp>) then
            buildTimestamp createInstance
        else
            struct (false, Unchecked.defaultof<obj>)
