namespace rec Tefin.Core.Reflection

open System
open System.Collections.Generic
open System.Reflection
open System.Threading
open Microsoft.FSharp.Core

module TypeBuilder =
    let private handlers =
        ResizeArray(
            [| SystemType.getDefault
               ArrayType.getDefault
               DictionaryType.getDefault
               GenericListType.getDefault
               ClassType.getDefault |]
        )

    let register handler = handlers.Insert(0, handler) //side-effect

    let getDefault (type2: Type) (createInstance: bool) (parentInstance: obj option) (depth: int) =
        let result =
            [ for h in handlers -> h ]
            |> Seq.fold
                (fun state handleFunc ->
                    let struct (handled: bool, _) = state

                    if handled then
                        state
                    else
                        let ret = handleFunc type2 createInstance parentInstance depth
                        ret)
                (struct (false, Unchecked.defaultof<obj>))

        result
//
// let getDefault (type2: Type) (createInstance: bool) (parentInstance: obj option) depth =
//     let mutable handled = false
//     let mutable instance = Unchecked.defaultof<obj>
//
//     for handleFunc in handlers do
//         if (not handled) then
//             let struct (h, i) = handleFunc type2 createInstance parentInstance depth
//             handled <- h
//             instance <- i

//    struct (handled, instance)

module SystemType =

    let private info =
        let markerToken = (new CancellationTokenSource()).Token
        let temp = Dictionary<Type, (unit -> obj) * string>()
        temp.Add(typeof<int>, ((fun () -> 0), "int"))
        temp.Add(typeof<int16>, ((fun () -> 0s), "int16"))
        temp.Add(typeof<int64>, ((fun () -> 0L), "long"))
        temp.Add(typeof<decimal>, ((fun () -> 0m), "dec"))
        temp.Add(typeof<Double>, ((fun () -> 0.0), "float"))
        temp.Add(typeof<Single>, ((fun () -> 0.0f), "float32"))
        temp.Add(typeof<uint>, ((fun () -> 0u), "uint"))
        temp.Add(typeof<uint16>, ((fun () -> 0us), "uint16"))
        temp.Add(typeof<uint64>, ((fun () -> 0UL), "ulong"))
        temp.Add(typeof<bool>, ((fun () -> true), "bool"))
        temp.Add(typeof<DateTime>, ((fun () -> DateTime.Now.AddDays 1), "dateTime"))
        temp.Add(typeof<DateTimeOffset>, ((fun () -> DateTimeOffset.Now.AddDays 1), "dtOffset"))
        temp.Add(typeof<Guid>, ((fun () -> Guid.NewGuid()), ""))
        temp.Add(typeof<TimeSpan>, ((fun () -> TimeSpan.FromSeconds 1), "timespan"))
        //temp.Add(typeof<CancellationToken>, ((fun () -> CancellationToken.None), "token"))
        temp.Add(typeof<CancellationToken>, ((fun () -> markerToken), "token"))
        temp.Add(typeof<string>, ((fun () -> ""), "string"))
        temp.Add(typeof<char>, ((fun () -> 'c'), "char"))
        temp.Add(typeof<byte>, ((fun () -> byte 0), "byte"))
        temp.Add(typeof<Uri>, ((fun () -> Uri("http://localhost:8080/")), "uri"))

        temp.Add(typeof<Nullable<int>>, ((fun () -> 0), "int?"))
        temp.Add(typeof<Nullable<int16>>, ((fun () -> 0), "int16?"))
        temp.Add(typeof<Nullable<int64>>, ((fun () -> 0), "long?"))
        temp.Add(typeof<Nullable<decimal>>, ((fun () -> 0), "dec?"))
        temp.Add(typeof<Nullable<Double>>, ((fun () -> 0), "float?"))
        temp.Add(typeof<Nullable<Single>>, ((fun () -> 0), "float32?"))
        temp.Add(typeof<Nullable<uint>>, ((fun () -> 0), "uint?"))
        temp.Add(typeof<Nullable<uint16>>, ((fun () -> 0), "uint16?"))
        temp.Add(typeof<Nullable<uint64>>, ((fun () -> 0), "ulong?"))
        temp.Add(typeof<Nullable<bool>>, ((fun () -> true), "bool?"))
        temp.Add(typeof<Nullable<DateTime>>, ((fun () -> DateTime.Now.AddDays 1), "dateTime?"))
        //temp.Add(typeof<Nullable<DateTime>>, ((fun () -> null), "dateTime?"))
        temp.Add(typeof<Nullable<DateTimeOffset>>, ((fun () -> DateTimeOffset.Now.AddDays 1), "dtOffset?"))
        temp.Add(typeof<Nullable<TimeSpan>>, ((fun () -> TimeSpan.FromSeconds 1), "timespan?"))
        temp.Add(typeof<Nullable<char>>, ((fun () -> 'c'), "char?"))
        temp

    let getDisplayName (thisType: Type) =
        let ok, (_, display) = info.TryGetValue(thisType)
        if ok then display else "not a system type"

    let isSystemType (thisType: Type) =
        let ok, _ = info.TryGetValue(thisType)
        ok

    let getDefault (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
        if thisType.IsEnum then
            let v = Enum.GetValues(thisType).GetValue(0)
            struct (true, v)
        elif info.ContainsKey(thisType) then
            let gen, _ = info[thisType]
            struct (true, gen ())
        else
            struct (false, TypeHelper.getDefault thisType)

module ArrayType =
    let getDefault (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
        if thisType.IsArray then
            let elementType = thisType.GetElementType()
            let instance = Array.CreateInstance(elementType, 1)

            let struct (ok, element) =
                TypeBuilder.getDefault elementType createInstance None depth

            if ok then
                instance.SetValue(element, 0)

            struct (true, instance)
        else
            struct (false, TypeHelper.getDefault thisType)

module GenericListType =
    let getDefault (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
        if (TypeHelper.isGenericListType thisType) && createInstance then
            let instance2 =
                let instance = Activator.CreateInstance(thisType)

                if not (instance = null) then
                    match TypeHelper.getListItemType thisType with
                    | Some elementType ->
                        let struct (ok, elementInstance) =
                            TypeBuilder.getDefault elementType true None depth

                        if ok then
                            let addMethod = thisType.GetMethod("Add", [| elementType |])
                            addMethod.Invoke(instance, [| elementInstance |]) |> ignore
                            instance
                        else
                            instance
                    | None -> instance
                else
                    instance

            struct (true, instance2)
        else
            struct (false, TypeHelper.getDefault thisType)

module ClassType =
    let private assignWritableProps (prop: PropertyInfo) (instance: obj) (isIndexParams: bool) depth =
        if isIndexParams then
            () //ignore indexed parameters like List[0]
        else
            let mutable objV = Unchecked.defaultof<obj>

            try
                let struct (_, newValue) = TypeBuilder.getDefault prop.PropertyType true None depth
                objV <- newValue
                prop.SetValue(instance, newValue)
            with exc ->
                let msg =
                    $"Unable to assign to {instance.GetType().Name}.{prop.Name} ({prop.PropertyType.Name}) = {objV}{Environment.NewLine}"

                System.Diagnostics.Debug.WriteLine(msg + exc.ToString())

    let private fillReadonlyProps (prop: PropertyInfo) (instance: obj) (isIndexParams: bool) (indexParams: ParameterInfo array) depth =
        if
            (isIndexParams
             && (indexParams[0].ParameterType = typeof<int>
                 || indexParams[0].ParameterType = typeof<int64>))
        then
            let count =
                let countInfo = instance.GetType().GetProperty("Count")

                if not (countInfo = null) then
                    Convert.ToInt32(countInfo.GetValue(instance))
                else
                    let lengthInfo = instance.GetType().GetProperty("Length")

                    if not (lengthInfo = null) then
                        Convert.ToInt32(lengthInfo.GetValue(instance))
                    else
                        0

            if (count > 0) then
                let a = prop.GetValue(instance, [| count - 1 |])
                let _ = TypeBuilder.getDefault prop.PropertyType false (Some(a)) depth
                ()
        else
            let currentInstance = prop.GetValue(instance)
            let _ = TypeBuilder.getDefault prop.PropertyType true (Some currentInstance) depth
            ()

    let fill (instance: obj) (depth: int) =
        if (depth > 3) then
            instance
        else
            let editableProps =
                instance.GetType().GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
                |> Array.filter (fun p -> p.CanRead)

            for prop in editableProps do
                let indexParams = prop.GetIndexParameters()
                let isIndexParams = not (indexParams = null) && indexParams.Length > 0

                if prop.CanWrite then
                    assignWritableProps prop instance isIndexParams depth
                elif prop.CanRead then
                    fillReadonlyProps prop instance isIndexParams indexParams depth
                else
                    ()

            instance

    let getDefault (thisType: Type) (createInstance: bool) (parentInstance: obj option) depth =
        if thisType.IsClass && not thisType.IsAbstract then
            if createInstance then
                let constructor = thisType.GetConstructor(Type.EmptyTypes)
                let mutable instance = Unchecked.defaultof<obj>

                if not (constructor = null) then
                    instance <- Activator.CreateInstance(thisType)
                    System.Diagnostics.Debug.WriteLine($"Calling constructor of {thisType.FullName}")
                    let _ = fill instance (depth + 1)
                    ()

                struct (true, instance)
            else
                struct (true, TypeHelper.getDefault thisType)
        else
            struct (false, TypeHelper.getDefault thisType)

module DictionaryType =
    let getDefault (thisType: Type) (createInstance: bool) (parentInstanceOpt: obj option) depth =
        if thisType.IsGenericType && (TypeHelper.isDictionaryType thisType) then
            let keyValTypes = thisType.GetGenericArguments()

            let parentInstance =
                if (createInstance || parentInstanceOpt.IsNone) then
                    Activator.CreateInstance(thisType)
                else
                    parentInstanceOpt.Value

            let struct (_, keyInstance) = TypeBuilder.getDefault keyValTypes[0] true None depth
            let struct (_, valInstance) = TypeBuilder.getDefault keyValTypes[1] true None depth

            let add =
                thisType.GetMethod("Add", BindingFlags.Public ||| BindingFlags.Instance, null, keyValTypes, null)

            add.Invoke(parentInstance, [| keyInstance; valInstance |]) |> ignore
            struct (true, parentInstance)
        else
            struct (false, TypeHelper.getDefault thisType)
