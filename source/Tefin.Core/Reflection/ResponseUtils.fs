namespace Tefin.Core.Reflection

open System.Collections.Concurrent
open System.Collections.Generic
open Grpc.Core
open Tefin.Core.Reflection
open Tefin.Core.Interop
open System
open System.Reflection

type TypeKey =
  { ClassName: string
    IsErrorContainer: bool }

module ResponseUtils =
  let emitErrorType (assemblyName: string) (moduleName: string) (className: string) =
    ClassGen.create
      assemblyName
      moduleName
      className
      [| { IsMethod = false
           Name = "Response"
           Type = typeof<ErrorResponse> } |]

  let emitStandardResponse =
    let generatedTypes = ConcurrentDictionary<TypeKey, Type>()

    fun (methodInfo: MethodInfo) (properties: PropInfo array) isContainerForError className ->
      let key = { ClassName = className; IsErrorContainer = isContainerForError }
      let genType = generatedTypes.GetOrAdd(key, fun _ ->
          let moduleName = className
          let assemblyName = $"Tefin{className}"
          let type2 =
            if isContainerForError then
              emitErrorType assemblyName moduleName className
            else
              ClassGen.create assemblyName moduleName className properties
          type2
        )
      genType

  let emitCompositeResponse prefix (getResponseType: MethodInfo -> Type) =
    let generatedTypes = ConcurrentDictionary<TypeKey, Type>()

    fun (methodInfo: MethodInfo) isContainerForError ->
      let className =
        $"{prefix}_{methodInfo.Name}_{methodInfo.ReturnType.Name}_{methodInfo.GetHashCode()}"

      let key =  { ClassName = className; IsErrorContainer = isContainerForError }
      let genType = generatedTypes.GetOrAdd(key, fun _ ->
          let moduleName = className
          let assemblyName = $"Tefin{className}"

          let responseType =
            if isContainerForError then
              typeof<ErrorResponse>
            else
              getResponseType methodInfo

          let propInfos =
            [| { IsMethod = false
                 Name = "Response"
                 Type = responseType }
               { IsMethod = false
                 Name = "Headers"
                 Type = typeof<Metadata> }
               { IsMethod = false
                 Name = "Trailers"
                 Type = typeof<Metadata> }
               { IsMethod = false
                 Name = "Status"
                 Type = typeof<Status> } |]

          ClassGen.create assemblyName moduleName className propInfos
        )
      genType       

  let emitClientStreamResponse =
    let getReturnType (methodInfo: MethodInfo) =
      methodInfo.ReturnType.GetGenericArguments().[1]

    fun (methodInfo: MethodInfo) isContainerForError ->
      let builder = emitCompositeResponse "GrpcClientStreamResponse" getReturnType
      builder methodInfo isContainerForError

  let emitUnaryResponse =
    let getReturnType (methodInfo: MethodInfo) =
      if methodInfo.ReturnType.IsGenericType then
        methodInfo.ReturnType.GetGenericArguments().[0]
      else
        methodInfo.ReturnType

    let generatedTypes = ConcurrentDictionary<TypeKey, Type>()

    fun (methodInfo: MethodInfo) (isAsync: bool) isContainerForError ->
      if isAsync then
        let builder = emitCompositeResponse "GrpcUnaryResponse" getReturnType
        builder methodInfo isContainerForError
      else
        let className = $"GrpcUnaryResponse_{methodInfo.Name}_{methodInfo.ReturnType.Name}_{methodInfo.GetHashCode()}"
        let moduleName = className
        let assemblyName = $"Tefin{className}"
        let key = { ClassName = className; IsErrorContainer = isContainerForError }
        let genType = generatedTypes.GetOrAdd(key, fun _ ->
            let respType =
              if isContainerForError then
                typeof<ErrorResponse>
              else
                getReturnType methodInfo
            ClassGen.create  assemblyName moduleName className  [| { IsMethod = false; Name = "Response"; Type = respType } |]
          )
        genType
 
//
//
// let className = $"GrpcUnaryResponse_{methodInfo.Name}_{methodInfo.ReturnType.Name}_{methodInfo.GetHashCode()}"
// let ok, v = generatedTypes.TryGetValue( { ClassName = className; IsErrorContainer = isContainerForError})
// if ok then v
// else
//     let moduleName = className
//     let assemblyName = $"Tefin{className}"
//     let responseType =
//         if isContainerForError then typeof<ErrorResponse>
//         else if methodInfo.ReturnType.IsGenericType then
//             methodInfo.ReturnType.GetGenericArguments().[0]
//         else
//             methodInfo.ReturnType
//     let propInfos =
//         if isAsync then
//             [| {IsMethod = false; Name = "Response"; Type = responseType }
//                {IsMethod = false; Name = "Headers"; Type = typeof<Metadata> }
//                {IsMethod = false; Name = "Trailers"; Type = typeof<Metadata> }
//                {IsMethod = false; Name = "Status"; Type = typeof<Status> }
//                 |]
//         else
//             [| {IsMethod = false; Name = "Response"; Type = responseType } |]
//     let genType =
//         ClassGen.create
//             assemblyName
//             moduleName
//             className
//             propInfos
//
//     let key = {ClassName = className; IsErrorContainer = isContainerForError}
//     generatedTypes.Add(key, genType)
//     genType
