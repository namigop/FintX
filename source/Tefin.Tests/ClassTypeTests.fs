module ClassTypeTests

open System
open Xunit

open Tefin.Core.Reflection
open Tefin.Tests.TestInputTypes

[<Fact>]
let ``Can build class instance`` () =
    let struct(ok, instance) = buildType typeof<Test1>
    let test1 = Test1()
    Assert.True ok
    Assert.NotNull instance
    Assert.Equal(typeof<Test1>, instance.GetType())


[<Fact>]
let ``Can build class containing other class`` () =
    let struct(ok, instance) = buildType typeof<Test2>
    let test = instance :?> Test2
    Assert.True ok
    Assert.NotNull instance
    Assert.NotNull test.Test1Type
    Assert.Equal(typeof<Test2>, instance.GetType())
    
    let struct(ok, o) = buildType typeof<Test1>
    let test1Instance = o :?> Test1
    let test1 = test.Test1Type
    Assert.Equal(test1.BoolType, test1Instance.BoolType)
    Assert.Equal(test1.CharType, test1Instance.CharType)
    Assert.Equal<string>(test1.StringType, test1Instance.StringType)
    Assert.Equal(test1.DateTimeType.Date, test1Instance.DateTimeType.Date)
    Assert.Equal(test1.TimeSpanType, test1Instance.TimeSpanType)
    Assert.Equal(test1.UriType, test1Instance.UriType)
    
    Assert.NotEqual(test1.DateTimeOffsetType, test1Instance.DateTimeOffsetType)
    Assert.NotEqual(test1.GuidType, test1Instance.GuidType)   

[<Fact>]
let ``Can set default values in class instance`` () =
    let struct(ok, instance) = buildType typeof<Test1>
    let test1 = Test1()
    let test1Instance = instance :?> Test1
    Assert.Equal(true, test1Instance.BoolType)
    Assert.NotEqual(test1.BoolType, test1Instance.BoolType)
    
    Assert.Equal('c', test1Instance.CharType)
    Assert.NotEqual(test1.CharType, test1Instance.CharType)
    
    Assert.Equal("", test1Instance.StringType)
    Assert.NotEqual<string>(test1.StringType, test1Instance.StringType)
    
    Assert.Equal(DateTime.Now.AddDays(1).Date, test1Instance.DateTimeType.Date)
    Assert.NotEqual(test1.DateTimeType, test1Instance.DateTimeType)
    
    Assert.Equal(TimeSpan.FromSeconds 1, test1Instance.TimeSpanType)
    Assert.NotEqual(test1.TimeSpanType, test1Instance.TimeSpanType)
    
    Assert.NotEqual(test1.DateTimeOffsetType, test1Instance.DateTimeOffsetType)
    Assert.Equal(Uri("http://localhost:8080"), test1Instance.UriType)
   
    Assert.NotEqual(test1.UriType, test1Instance.UriType)
    Assert.NotEqual(test1.GuidType, test1Instance.GuidType)
    