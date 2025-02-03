namespace Tefin.Core.Json

open System
open Grpc.Core
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type StatusConverter() =
  inherit JsonConverter()

  override x.CanRead = true
  override x.CanWrite = false
  override x.CanConvert(objectType: Type) = objectType = typeof<Status>

  override x.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
    //Not needed since CanWrite= false
    failwith "not supported"

  override x.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer) =
    // Load the JSON for the Result into a JObject
    let jo = JObject.Load(reader)
    let k = jo["StatusCode"].ToObject<StatusCode>()
    let v = jo["Detail"].ToObject<string>()
    let d = jo["DebugException"].ToObject<Exception>()
    Status(k, v, d)

type StatusConverter2() =
  inherit System.Text.Json.Serialization.JsonConverter<Status>()

  override x.CanConvert(objectType: Type) = objectType = typeof<Status>

  override x.Read(reader: byref<System.Text.Json.Utf8JsonReader>, typeToConvert: Type, options: System.Text.Json.JsonSerializerOptions) =
    // let doc = System.Text.Json.JsonDocument.ParseValue(&reader)
    // let root = doc.RootElement
    // let k = root.GetProperty("StatusCode").Deserialize<StatusCode>(options)
    // let v = root.GetProperty("Detail").GetString()
    // let d = root.GetProperty("DebugException").Deserialize<Exception>(options)
    
    let doc = System.Text.Json.JsonDocument.ParseValue(&reader)
    let root = doc.RootElement
    let k = System.Text.Json.JsonSerializer.Deserialize<StatusCode>(root.GetProperty("StatusCode").GetRawText(), options)
    let v = root.GetProperty("Detail").GetString()
    let d = System.Text.Json.JsonSerializer.Deserialize<Exception>(root.GetProperty("DebugException").GetRawText(), options)
    
    Status(k, v, d)

  override x.Write(writer: System.Text.Json.Utf8JsonWriter, value: Status, options: System.Text.Json.JsonSerializerOptions) =
    raise (System.NotSupportedException("Writing is not supported"))