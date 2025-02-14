namespace Tefin.Core.Json

open System
open Grpc.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type MetadataEntryConverter() =
  inherit JsonConverter()

  override x.CanRead = true
  override x.CanWrite = false
  override x.CanConvert(objectType: Type) = objectType = typeof<Metadata.Entry>

  override x.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
    //Not needed since CanWrite= false
    failwith "not supported"

  override x.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer) =
    // Load the JSON for the Result into a JObject
    let jo = JObject.Load(reader)
    let k = jo["Key"].ToObject<string>()
    let v = jo["Value"].ToObject<string>()
    let entry = Metadata.Entry(k, v)
    entry

type MetadataEntryConverter2() =
  inherit System.Text.Json.Serialization.JsonConverter<Metadata.Entry>()

  override x.CanConvert(objectType: Type) = objectType = typeof<Metadata.Entry>

  override x.Read(reader: byref<System.Text.Json.Utf8JsonReader>, typeToConvert: Type, options: System.Text.Json.JsonSerializerOptions) =
    let doc = System.Text.Json.JsonDocument.ParseValue(&reader)
    let root = doc.RootElement
    let k = root.GetProperty("Key").GetString()
    let v = root.GetProperty("Value").GetString()
    Metadata.Entry(k, v)

  override x.Write(writer: System.Text.Json.Utf8JsonWriter, value: Metadata.Entry, options: System.Text.Json.JsonSerializerOptions) =
    //raise (System.NotSupportedException("Writing is not supported"))
    writer.WriteStartObject()
       
    
    writer.WritePropertyName("Key")
    let conv = options.GetConverter(typeof<string>) :?> System.Text.Json.Serialization.JsonConverter<string>
    conv.Write(writer, value.Key, options)
    
    writer.WritePropertyName("Value")    
    conv.Write(writer, value.Value, options)
     
    writer.WriteEndObject();