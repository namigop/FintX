namespace Tefin.Core.Json

open System
open Google.Protobuf
open Grpc.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type MetadataEntryConverter() =
    inherit JsonConverter()
    
    override x.CanRead = true
    override x.CanWrite = false
    override x.CanConvert(objectType:Type) =   objectType = typeof<Metadata.Entry>
    override x.WriteJson(writer:JsonWriter, value:obj, serializer:JsonSerializer) =
        //Not needed since CanWrite= false
        failwith "not supported"
    
    override x.ReadJson(reader:JsonReader, objectType:Type, existingValue:obj, serializer:JsonSerializer) =
        // Load the JSON for the Result into a JObject
        let jo = JObject.Load(reader)
        let k = jo["Key"].ToObject<string>()
        let v = jo["Value"].ToObject<string>()
        let entry = Metadata.Entry(k, v)
        entry

        // Read the properties which will be used as constructor parameters
        //int? code = (int?)jo["Code"];
        //string format = (string)jo["Format"];


