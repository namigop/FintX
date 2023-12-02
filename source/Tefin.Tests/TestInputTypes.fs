module Tefin.Tests.TestInputTypes

open System
open System.Collections.Generic
open System.Threading
open Tefin.Core.Reflection
 
type Test1() =
    member val IntType = 0 with get, set
    member val Int16Type = 0s with get, set
    member val Int64Type = 0L with get, set
    member val DoubleType = 0.0 with get, set
    member val DecimalType = 0.0m with get, set
    member val SingleType = 0f with get, set
    member val UIntType = 0u with get, set
    member val UInt16Type = 0us with get, set
    member val UInt64Type = 0uL with get, set
    member val BoolType = false with get, set
    member val DateTimeType = Unchecked.defaultof<DateTime> with get, set
    member val DateTimeOffsetType = Unchecked.defaultof<DateTimeOffset> with get, set
    member val GuidType = Unchecked.defaultof<Guid> with get, set
    member val TimeSpanType = Unchecked.defaultof<TimeSpan>  with get, set
    member val CancellationTokenType =  Unchecked.defaultof<CancellationToken> with get, set
    member val UriType =  Unchecked.defaultof<Uri> with get, set
    member val CharType =  Unchecked.defaultof<char> with get, set
    member val ByteType =  10uy with get, set   
    member val StringType = "abc" with get, set


type Test2() =
    member val Test1Type = Test1() with get, set
    //member val ArrayType = [| Test1() |] with get, set

type Test3() =
    member val ArrayType = [|Test1()|] with get, set
    member val GenericListType = ResizeArray<Test1>([|Test1()|]) with get, set
    member val DictType = Dictionary<Test1, Test2>() with get, set

type Test4() =
    member val DictOfListArrayType =  Dictionary<Test1 array, ResizeArray<Test3>>() with get, set
     
    
let buildType t =
    TypeBuilder.getDefault t true None 0