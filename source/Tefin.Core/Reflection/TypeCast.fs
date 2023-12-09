namespace Tefin.Core.Reflection

open System

[<AbstractClass; Sealed>]
type TypeCast private () =
    static member Cast<'T>(o: obj) =
        match o with
        | :? 'T as r -> r
        | _ ->
            let name = typeof<'T>.Name
            failwith $"The value {o} is not a valid {name}"

    static member CastTo (targetType:Type) (instance:obj) =
        let t = typeof<TypeCast>
        let mi = t.GetMethod("Cast")
        let generic = mi.MakeGenericMethod targetType
        generic.Invoke(null, [|instance|])
        
    static member GetDefault<'T>() =

        Unchecked.defaultof<'T>
// if (typeof<T> = typeof<int>) then
//      o = Int32.Parse(o.ToString());