namespace Tefin.Core.Reflection

open System
open System.Globalization

[<AbstractClass; Sealed>]
type TypeCast private () =
  
  static member Cast<'T>(o: obj) =
    let unwrap o = (box o) |> unbox<'T>
    match o with
    | :? 'T as r -> r
    | _ ->       
       match typeof<'T> with
        | t when t = typeof<int>      -> unwrap (Int32.Parse(o.ToString()))
        | t when t = typeof<bool>     -> unwrap (Convert.ToBoolean(o))
        | t when t = typeof<string>   -> unwrap (o.ToString())
        | t when t = typeof<Guid>     -> unwrap (Guid(o.ToString()))
        | t when t = typeof<double>   -> unwrap (Convert.ToDouble(o))
        | t when t = typeof<decimal>  -> unwrap (Convert.ToDecimal(o))
        | t when t = typeof<int16>    -> unwrap (Convert.ToInt16(o))
        | t when t = typeof<int64>    -> unwrap (Convert.ToInt64(o))
        | t when t = typeof<single>   -> unwrap (Convert.ToSingle(o))
        | t when t = typeof<uint16>   -> unwrap (Convert.ToUInt16(o))
        | t when t = typeof<uint32>   -> unwrap (Convert.ToUInt32(o))
        | t when t = typeof<uint64>   -> unwrap (Convert.ToUInt64(o))
        | t when t = typeof<char>     -> unwrap (Convert.ToChar(o))
        | t when t = typeof<DateTime> -> unwrap (DateTime.Parse(o.ToString(), CultureInfo.CurrentCulture))
        | t when t = typeof<DateTimeOffset> -> unwrap (DateTimeOffset.Parse(o.ToString(), CultureInfo.CurrentCulture))
        | t when t = typeof<Uri>      -> unwrap (Uri(o.ToString()))        
        | t when t = typeof<Nullable<int>>      -> unwrap (Int32.Parse(o.ToString()))
        | t when t = typeof<Nullable<bool>>     -> unwrap (Convert.ToBoolean(o))        
        | t when t = typeof<Nullable<Guid>>     -> unwrap (Guid(o.ToString()))
        | t when t = typeof<Nullable<double>>   -> unwrap (Convert.ToDouble(o))
        | t when t = typeof<Nullable<decimal>>  -> unwrap (Convert.ToDecimal(o))
        | t when t = typeof<Nullable<int16>>    -> unwrap (Convert.ToInt16(o))
        | t when t = typeof<Nullable<int64>>    -> unwrap (Convert.ToInt64(o))
        | t when t = typeof<Nullable<single>>   -> unwrap (Convert.ToSingle(o))
        | t when t = typeof<Nullable<uint16>>   -> unwrap (Convert.ToUInt16(o))
        | t when t = typeof<Nullable<uint32>>   -> unwrap (Convert.ToUInt32(o))
        | t when t = typeof<Nullable<uint64>>   -> unwrap (Convert.ToUInt64(o))
        | t when t = typeof<Nullable<char>>     -> unwrap (Convert.ToChar(o))
        | t when t = typeof<Nullable<DateTime>> -> unwrap (DateTime.Parse(o.ToString(), CultureInfo.CurrentCulture))
        | t when t = typeof<Nullable<DateTimeOffset>> -> unwrap (DateTimeOffset.Parse(o.ToString(), CultureInfo.CurrentCulture))
        | t when t.FullName.StartsWith("System.Nullable") ->
          let underlyingType = Nullable.GetUnderlyingType(typeof<'T>)
          let helperType = typeof<TypeCast>
          let m = helperType.GetMethod("Cast")
          let methodInfo = m.MakeGenericMethod(underlyingType)
          methodInfo.Invoke(null, [| o |]) |> unwrap
        | _ ->            
          let name = typeof<'T>.Name
          failwith $"The value {o} is not a valid {name}"

  static member CastTo (targetType: Type) (instance: obj) =
    let t = typeof<TypeCast>
    let mi = t.GetMethod("Cast")
    let generic = mi.MakeGenericMethod targetType
    generic.Invoke(null, [| instance |])

  static member GetDefault<'T>() = Unchecked.defaultof<'T>
// if (typeof<T> = typeof<int>) then
//      o = Int32.Parse(o.ToString());
