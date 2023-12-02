namespace Tefin.Core.Reflection

[<AbstractClass; Sealed>]
type TypeCast private () =
    static member Cast<'T>(o: obj) =
        match o with
        | :? 'T as r -> r
        | _ ->
            let name = typeof<'T>.Name
            failwith $"The value {o} is not a valid {name}"

    static member GetDefault<'T>() =

        Unchecked.defaultof<'T>
// if (typeof<T> = typeof<int>) then
//      o = Int32.Parse(o.ToString());