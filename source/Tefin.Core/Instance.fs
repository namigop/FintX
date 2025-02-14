namespace Tefin.Core

open System
open System.IO
open Newtonsoft.Json
open Tefin.Core.Json
open Tefin.Core.Reflection

type PropInstance = { Value: obj; PropInfoName: string }


module Instance =

  let private jsonSettings =
    let settings = JsonSerializerSettings()
    settings.Converters.Add(ByteStringConverter())
    settings.Converters.Add(MetadataEntryConverter())
    settings.Converters.Add(StatusConverter())
    settings.Converters.Add(CancellationTokenConverter())
    settings

  let jsonOptions =
    let settings = new System.Text.Json.JsonSerializerOptions( WriteIndented = true);
    settings.Converters.Add(ByteStringConverter2())
    settings.Converters.Add(MetadataEntryConverter2())
    settings.Converters.Add(StatusConverter2())
    settings.Converters.Add(CancellationTokenConverter2())
    settings
    
  let jsonSerialize<'T when 'T: equality> (objToSerialize: 'T) =
    try
      if not (objToSerialize = Unchecked.defaultof<'T>) then
        //System.Text.Json.JsonSerializer.Serialize(objToSerialize, jsonOptions)
        use sw = new StringWriter()
        use jw = new JsonTextWriter(sw, Formatting = Formatting.Indented, Indentation = 4)
        let ser = JsonSerializer.Create jsonSettings // .CreateDefault()
        ser.Serialize(jw, objToSerialize)
        let json = sw.ToString()
        json
      else
        ""
    with exc ->
      Log.logError (exc.ToString())
      ""


  let jsonDeserialize<'T> (json: string) =
    try
      let obj = JsonConvert.DeserializeObject<'T>(json, jsonSettings)
      //let obj = System.Text.Json.JsonSerializer.Deserialize<'T>(json, jsonOptions)
      obj
    with exc ->
      Log.logError (exc.ToString())
      Unchecked.defaultof<'T>

  //this is just a marker type
  type Dummy = { A: int }

  let indirectDeserialize (targetType: Type) (json: string) : obj =
    let helperType = typeof<Dummy>
    let m = helperType.ReflectedType.GetMethod("jsonDeserialize")
    let methodInfo = m.MakeGenericMethod(targetType)
    let instance = methodInfo.Invoke(null, [| json |])
    instance

  let indirectSerialize (targetType: Type) (instance: obj) : string =
    let helperType = typeof<Dummy>
    let m = helperType.ReflectedType.GetMethod("jsonSerialize")
    let methodInfo = m.MakeGenericMethod(targetType)
    let typedInstance = TypeCast.CastTo targetType instance
    let json = methodInfo.Invoke(null, [| typedInstance |])
    json.ToString()

  let fromJson (json: string) (generatedType: Type) =
    Res.exec (fun () -> indirectDeserialize generatedType json)

  let toJson (props: PropInstance array) (generatedType: Type) =
    Res.exec (fun () ->
      let instance = Activator.CreateInstance(generatedType)

      for p in props do
        let pi = generatedType.GetProperty(p.PropInfoName)
        pi.SetValue(instance, p.Value)

      indirectSerialize generatedType instance)
