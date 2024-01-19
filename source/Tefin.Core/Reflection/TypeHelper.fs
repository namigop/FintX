namespace Tefin.Core.Reflection

open System
open System.IO
open System.Linq.Expressions
open System.Collections.Generic
//open Google.Protobuf.Collections
open Tefin.Core

module TypeHelper =

    let getListItemType (type2: Type) =
        type2.GetInterfaces()
        |> Array.tryFind (fun i -> i.IsGenericType)
        |> fun i ->
            match i with
            | Some x -> x.GenericTypeArguments |> Array.tryHead
            | None -> None

    let hasSubclasses (type2: Type) =
        let c = type2.Assembly.GetTypes() |> Array.tryFind (fun t -> t.IsSubclassOf(type2))
        c.IsSome

    //let isBottomNodeType (runtimeType: Type) =

    let isDictionaryType (type2: Type) =
        typeof<System.Collections.IDictionary>.IsAssignableFrom(type2)

    let isGenericCollectionType (typ2: Type) =
        typ2.GetInterfaces()
        |> Array.filter (fun i -> i.IsGenericType)
        |> Array.tryFind (fun i -> i.GetGenericTypeDefinition() |> typedefof<ICollection<_>>.IsAssignableFrom)
        |> fun t -> t.IsSome

    let isGenericListType (typ2: Type) =
        typ2.GetInterfaces()
        |> Array.filter (fun i -> i.IsGenericType)
        |> Array.tryFind (fun i ->
            let isList () =
                i.GetGenericTypeDefinition() |> typedefof<IList<_>>.IsAssignableFrom

            let isEnumerable () =
                i.GetGenericTypeDefinition() |> typedefof<IEnumerable<_>>.IsAssignableFrom

            isList () || isEnumerable ())
        |> fun t -> t.IsSome

    let isNullable type2 =
        not ((Nullable.GetUnderlyingType type2) = null)

    let isOfType_<'t> (type2: Type) = typeof<'t>.IsAssignableFrom type2

    let isOfType (type1: Type) (targetType: Type) = targetType.IsAssignableFrom type1

    let getFormattedGenericName (genericType: Type) =
        let name = genericType.Name.Split("`")[0]
        let args = genericType.GenericTypeArguments |> Array.map (fun t -> t.Name)
        let f = String.Join(", ", args)
        $"{name}<{f}>"

    let getMethodInfo (expression: Expression<Action>) =
        let mem = expression.Body :?> MethodCallExpression
        mem.Method

    let getTypesFromBytes (bytes: byte array) =
        use ms = new MemoryStream(bytes)
        ms.Seek(0, SeekOrigin.Begin) |> ignore
        let context = LoadContext()
        let assembly = context.LoadFromStream(ms)
        assembly.GetTypes()

    let getTypes (io: IOResolver) (dll: string) =
        task {
            let! bytes = io.File.ReadAllBytesAsync(dll)
            return getTypesFromBytes bytes
        }

    let indirectCast (instance: obj) (targetType: Type) =
        if (instance = null) then
            Unchecked.defaultof<obj>
        else
            let helperType = typeof<TypeCast>
            let m = helperType.GetMethod("Cast")
            let methodInfo = m.MakeGenericMethod(targetType)
            methodInfo.Invoke(null, [| instance |])

    let createGenericInstanceType (targetGenericType: Type) (typeArgs: Type array) (instance: obj) =
        let t = targetGenericType.MakeGenericType(typeArgs)
        indirectCast (instance, t)

    let createGenericList (itemType: Type) =
        let listType = typeof<List<_>>
        let constructedListType = listType.MakeGenericType(itemType)
        let instance = Activator.CreateInstance(constructedListType)
        instance

    let getDefault (targetType: Type) =
        let helperType = typeof<TypeCast>
        let m = helperType.GetMethod("GetDefault")
        let methodInfo = m.MakeGenericMethod(targetType)
        methodInfo.Invoke(null, null)
//Tefin.Core.Reflection.SystemType
