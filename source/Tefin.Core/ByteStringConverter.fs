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

type ByteStringConverter2() =
  inherit System.Text.Json.Serialization.JsonConverter<ByteString>()

  override x.CanConvert(objectType: Type) = objectType = typeof<ByteString>

  override x.Read(reader: byref<System.Text.Json.Utf8JsonReader>, typeToConvert: Type, options: System.Text.Json.JsonSerializerOptions) =
     let bytes = ResizeArray<byte>()
     while reader.TokenType = System.Text.Json.JsonTokenType.Number do
       bytes.Add(reader.GetByte())
       reader.Read() |> ignore

     ByteString.CopyFrom (Seq.toArray bytes)

  override x.Write(writer: System.Text.Json.Utf8JsonWriter, value: ByteString, options: System.Text.Json.JsonSerializerOptions) =
    // Not implemented since conversion is one-way
    raise (System.NotSupportedException("Writing is not supported"))