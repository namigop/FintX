module CollectionTypesTest

open System
open System.Collections.Generic
//open Microsoft.FSharp.Collections
open Xunit

//open Tefin.Core.Reflection
open Tefin.Tests.TestInputTypes

[<Fact>]
let ``Can build array instance`` () =
    let struct(ok, instance) = buildType typeof<Test1 array>
    Assert.True ok
    Assert.Equal(typeof<Test1 array>, instance.GetType())
    let arrayInstance = instance :?> Test1 array
    Assert.Equal(1, arrayInstance.Length)
    Assert.NotNull(arrayInstance[0])
    
[<Fact>]
let ``Can build List<T> instance`` () =
    let struct(ok, instance) = buildType typeof<List<Test1>>
    Assert.True ok
    Assert.Equal(typeof<ResizeArray<Test1>>, instance.GetType())
    let listInstance = instance :?> ResizeArray<Test1>
    Assert.Equal(1, listInstance.Count)
    Assert.NotNull(listInstance[0])   
    
[<Fact>]
let ``Can build Dictionary<T> instance`` () =
    let struct(ok, instance) = buildType typeof<Dictionary<string,Test1>>
    Assert.True ok
    Assert.Equal(typeof<Dictionary<string, Test1>>, instance.GetType())
    let listInstance = instance :?> Dictionary<string, Test1>
    Assert.Equal(1, listInstance.Count)
    Assert.NotNull( Seq.head listInstance.Values)
    
[<Fact>]
let ``Can build Array of array`` () =
    let struct(ok, instance) = buildType typeof<(Test1 array) array>
    Assert.True ok
    let arrayOfArray = instance :?> Test1 array array
    Assert.Equal(1, arrayOfArray.Length)
    let arrayItem = arrayOfArray[0]
    Assert.Equal(1, arrayItem.Length)
    let item = arrayItem[0]
    Assert.NotNull(item)
 
     
[<Fact>]
let ``Can build List of List`` () =
    let struct(ok, instance) = buildType typeof<ResizeArray<ResizeArray<Test2>>>
    Assert.True ok
    let listOfList = instance :?> ResizeArray<ResizeArray<Test2>>
    Assert.Equal(1, listOfList.Count)
    let listItem = listOfList[0]
    Assert.Equal(1, listItem.Count)
    let item = listItem[0]
    Assert.NotNull(item)
    Assert.NotNull(item.Test1Type)
    
    
[<Fact>]
let ``Can build class with collections`` () =
    let struct(ok, instance) = buildType typeof<Test3>
    Assert.True ok
    Assert.NotNull instance
    
[<Fact>]
let ``Can build complex class`` () =
    let struct(ok, instance) = buildType typeof<Test4>
    Assert.True ok
    Assert.NotNull instance
    
    let test4 = instance :?> Test4
    Assert.NotNull (test4.DictOfListArrayType)
    Assert.Equal(1,test4.DictOfListArrayType.Count)
    Assert.Equal(1,test4.DictOfListArrayType.Keys.Count)
    Assert.Equal(1,test4.DictOfListArrayType.Values.Count)
    
    let keyInstance = test4.DictOfListArrayType.Keys |> Seq.head
    Assert.Equal(1, keyInstance.Length)
    Assert.NotNull keyInstance[0]
    
    let valInstance = test4.DictOfListArrayType.Values |> Seq.head
    Assert.Equal(1, valInstance.Count)
    Assert.NotNull valInstance[0]
    Assert.Equal(1, valInstance[0].ArrayType.Length)
    Assert.NotNull(valInstance[0].ArrayType[0])
    