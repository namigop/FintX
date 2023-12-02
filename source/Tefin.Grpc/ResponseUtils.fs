namespace Tefin.Grpc.Execution

open System.Collections.Generic
open Tefin.Core.Reflection
open Tefin.Core.Interop
open System
open System.Reflection

type TypeKey = {
    ClassName : string
    IsErrorContainer : bool
}

module ResponseUtils =
    let emitErrorType (assemblyName:string) (moduleName:string) (className:string) =
        ClassGen.create
            assemblyName
            moduleName
            className
            [| {IsMethod = false; Name = "Response"; Type = typeof<ErrorResponse> } |]
    let emitStandardResponse =
        let generatedTypes = Dictionary<TypeKey, Type>()
        fun (methodInfo:MethodInfo) (properties : PropInfo array) isContainerForError className ->
            let ok, v = generatedTypes.TryGetValue( { ClassName = className; IsErrorContainer = isContainerForError})
            if ok then v
            else
                let moduleName = className
                let assemblyName = $"Tefin{className}"
                let type2 =
                    if isContainerForError then
                        emitErrorType assemblyName moduleName className
                    else
                        ClassGen.create
                            assemblyName
                            moduleName
                            className
                            properties
                generatedTypes.Add({ClassName = className; IsErrorContainer = isContainerForError}, type2)
                type2
    let emitUnaryResponse =
        let generatedTypes = Dictionary<TypeKey, Type>()
        fun (methodInfo:MethodInfo) isContainerForError ->
            let className = $"GrpcUnaryResponse_{methodInfo.Name}_{methodInfo.ReturnType.Name}_{methodInfo.GetHashCode()}"
            let moduleName = className
            let assemblyName = $"Tefin{className}"
            let responseType =
                if isContainerForError then typeof<ErrorResponse>
                else if methodInfo.ReturnType.IsGenericType then
                    methodInfo.ReturnType.GetGenericArguments().[0]
                else
                    methodInfo.ReturnType
            ClassGen.create
                assemblyName
                moduleName
                className
                [| {IsMethod = false; Name = "Response"; Type = responseType } |]