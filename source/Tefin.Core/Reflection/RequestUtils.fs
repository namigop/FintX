namespace Tefin.Core.Reflection

open System.Collections.Generic
open Tefin.Core.Reflection
open System
open System.Reflection

module RequestUtils =
  let emitRequest =
    let generatedTypes = Dictionary<string, Type>()

    fun (getClassName: unit -> string) (getProperties: unit -> PropInfo array) ->
      let className = getClassName ()
      let ok, v = generatedTypes.TryGetValue(className)

      if ok then
        v
      else
        let moduleName = className
        let assemblyName = $"Tefin{className}"
        let genType = ClassGen.create assemblyName moduleName className (getProperties ())
        generatedTypes.Add(className, genType)
        genType

  let emitRequestClass (prefix: string) (methodInfo: MethodInfo) =
    let getClassName () =
      let className =
        $"{prefix}_Request__{methodInfo.Name}_{methodInfo.ReturnType.Name}_{methodInfo.GetHashCode()}"

      className

    let getProperties () =
      methodInfo.GetParameters()
      |> Array.map (fun pi ->
        { IsMethod = false
          Name = pi.Name
          Type = pi.ParameterType })

    emitRequest getClassName getProperties


  let emitStreamingRequestClass (prefix: string) (methodInfo: MethodInfo) =
    let requestType = methodInfo.ReturnType.GetGenericArguments()[0]

    let getClassName () =
      let className =
        $"{prefix}_{methodInfo.Name}_{methodInfo.ReturnType.Name}_{requestType.Name}_{methodInfo.GetHashCode()}"

      className

    let getProperties () =
      [| { IsMethod = false
           Name = "Request"
           Type = requestType } |]

    emitRequest getClassName getProperties
