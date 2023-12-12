namespace Tefin.Core.Json

open System
open Google.Protobuf
open Grpc.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type StatusConverter() =
    inherit JsonConverter()
    
    override x.CanRead = true
    override x.CanWrite = false
    override x.CanConvert(objectType:Type) =   objectType = typeof<Status>
    override x.WriteJson(writer:JsonWriter, value:obj, serializer:JsonSerializer) =
        //Not needed since CanWrite= false
        failwith "not supported"
    
    override x.ReadJson(reader:JsonReader, objectType:Type, existingValue:obj, serializer:JsonSerializer) =
        // Load the JSON for the Result into a JObject
        let jo = JObject.Load(reader)
        let k = jo["StatusCode"].ToObject<StatusCode>()
        let v = jo["Detail"].ToObject<string>()
        let d = jo["DebugException"].ToObject<Exception>()
        Status(k,v, d)
        
 

