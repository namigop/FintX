namespace Tefin.Core.Json

open System
open Google.Protobuf
open Newtonsoft.Json

type ByteStringConverter() =
  inherit JsonConverter()

  override x.CanRead = true
  override x.CanWrite = false
  override x.CanConvert(objectType: Type) = objectType = typeof<ByteString>

  override x.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
    //Not needed since CanWrite= false
    failwith "not supported"

  override x.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer) =
    let rec read (r: JsonReader) (bytes: ResizeArray<int>) =
      let byteVal = r.ReadAsInt32()

      if byteVal.HasValue then
        bytes.Add byteVal.Value

      if (r.TokenType = JsonToken.Integer) then
        read r bytes
      else
        bytes

    let bytes =
      read reader (ResizeArray())
      |> Seq.map (fun c -> Convert.ToByte c)
      |> Seq.toArray

    ByteString.CopyFrom bytes
