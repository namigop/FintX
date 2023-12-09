namespace Tefin.Core

open System
open System.Reflection
open Newtonsoft.Json
open Tefin.Core.Reflection

type PropInstance = {
    Value:obj
    PropInfoName : string
}

    
module Instance =
        
    let jsonSerialize<'T when 'T:equality>(objToSerialize:'T) =
       if not (objToSerialize = Unchecked.defaultof<'T>) then
          let json = JsonConvert.SerializeObject(objToSerialize, Formatting.Indented)
          json
       else
          ""
    let jsonDeserialize<'T>(json:string) =
       try
          let settings = JsonSerializerSettings()
          //settings.Converters.Add(new ByteStringConverter());
          let obj = JsonConvert.DeserializeObject<'T>(json, settings)
          obj
       with
       | exc ->
           Console.WriteLine (exc.ToString())
           Unchecked.defaultof<'T>
       
    //this is just a marker type       
    type Dummy = {
        A :int
    }
    let indirectDeserialize (targetType:Type) (json: string) : obj  =
        let helperType = typeof<Dummy>
        let m = helperType.ReflectedType.GetMethod("jsonDeserialize");
        let methodInfo = m.MakeGenericMethod(targetType)        
        let instance = methodInfo.Invoke(null, [| json  |] )
        instance
        
    let indirectSerialize (targetType:Type) (instance: obj) : string  =
        let helperType = typeof<Dummy>
        let m = helperType.ReflectedType.GetMethod("jsonSerialize");
        let methodInfo = m.MakeGenericMethod(targetType)
        let typedInstance = TypeCast.CastTo targetType instance
        let json = methodInfo.Invoke(null, [| typedInstance  |] )
        json.ToString()

    let fromJson (json:string) (generatedType:Type) =
        indirectDeserialize generatedType json
        
    let toJson (props: PropInstance array) (generatedType:Type) =
        let instance = Activator.CreateInstance(generatedType)
        for p in props do
            let pi = generatedType.GetProperty(p.PropInfoName)
            pi.SetValue(instance, p.Value)
       
        indirectSerialize generatedType instance    
            

