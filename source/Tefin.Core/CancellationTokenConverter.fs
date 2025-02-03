namespace Tefin.Core.Json

open System
open System.Threading
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type CancellationTokenConverter() =
  inherit JsonConverter()

  override x.CanRead = true
  override x.CanWrite = true
  override x.CanConvert(objectType: Type) = objectType = typeof<CancellationToken>

  override x.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
    let jo = JObject()
    jo.Add("value", "none")
    jo.WriteTo(writer)

  override x.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer) =
    let jo = JObject.Load(reader)
    CancellationToken.None
type CancellationTokenConverter2() =
 inherit System.Text.Json.Serialization.JsonConverter<CancellationToken>()

 override x.CanConvert(objectType: Type) = objectType = typeof<CancellationToken>

 override x.Write(writer: System.Text.Json.Utf8JsonWriter, value: CancellationToken, options: System.Text.Json.JsonSerializerOptions) =
   writer.WriteStartObject()
   writer.WriteString("value", "none")
   writer.WriteEndObject()

 override x.Read(reader: byref<System.Text.Json.Utf8JsonReader>, typeToConvert: Type, options: System.Text.Json.JsonSerializerOptions) =
   System.Text.Json.JsonDocument.ParseValue(&reader) |> ignore
   CancellationToken.None