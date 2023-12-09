namespace Tefin.Core.Json

open System
open System.Threading
open Google.Protobuf
open Grpc.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type CancellationTokenConverter() =
    inherit JsonConverter()
    
    override x.CanRead = true
    override x.CanWrite = false
    override x.CanConvert(objectType:Type) =   objectType = typeof<CancellationToken>
    override x.WriteJson(writer:JsonWriter, value:obj, serializer:JsonSerializer) =
        //Not needed since CanWrite= false
        failwith "not supported"
    
    override x.ReadJson(reader:JsonReader, objectType:Type, existingValue:obj, serializer:JsonSerializer) =
         CancellationToken.None


