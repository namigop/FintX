module SerializeDeseializeTests

open System.Threading
open Newtonsoft.Json.Linq
open Tefin.Core
open Tefin.Grpc.Dynamic
open Tefin.Views.Types
open Xunit
open Tefin.Tests.TestInputTypes

type Sample() =    
    member x.RunTest1(arg:Test1, intArg:int, stringArg:string, doubleArg:float) =
        arg

[<Fact>]
let ``Can serialize methodArgs tree`` () = 
    let mi = typeof<Sample>.GetMethod("RunTest1")
    let struct (ok,test1) = buildType typeof<Test1>
    let intVal = 42
    let stringVal = "foobar"
    let doubleVal = 42.42
    
    let args = [|test1; intVal; stringVal; doubleVal|]
    let jsonRet = DynamicTypes.toJsonRequest { Method = mi; RequestParams = args; EnvVariables= Array.empty; RequestStream = None }
    Assert.True (Res.isOk jsonRet)
   
    //Validate the generate json
    let jObj = JObject.Parse (Res.getValue jsonRet)
    let argToken = jObj.SelectToken "$.arg"
    Assert.True argToken.HasValues
   
    let intArgToken = jObj.SelectToken "$.intArg"
    Assert.False intArgToken.HasValues
    let t = intArgToken.ToObject()
    Assert.Equal(intVal, t)
    
    let stringArgToken = jObj.SelectToken "$.stringArg"
    Assert.False stringArgToken.HasValues
    let t = stringArgToken.ToObject()
    Assert.Equal(stringVal, t)
    
    let doubleArgToken = jObj.SelectToken "$.doubleArg"
    Assert.False doubleArgToken.HasValues
    let t = doubleArgToken.ToObject()
    Assert.Equal(doubleVal, t)


 
[<Fact>]
let ``Can deserialize methodArgs tree`` () =
    let mi = typeof<Sample>.GetMethod("RunTest1")
    let struct (ok,test1) = buildType typeof<Test1>
    let intVal = 42
    let stringVal = "foobar"
    let doubleVal = 42.42
    
    let args = [|test1; intVal; stringVal; doubleVal|]
    let jsonRet = DynamicTypes.toJsonRequest { Method = mi; RequestParams = args; EnvVariables= Array.empty; RequestStream = None }
    Assert.True(Res.isOk jsonRet)
    let genInstanceRet = DynamicTypes.fromJsonRequest mi (Res.getValue jsonRet)
    Assert.True (Res.isOk genInstanceRet)
    
    let genInstance = Res.getValue genInstanceRet
    let argNames = [ "intArg"; "stringArg"; "doubleArg"]
    let args = [ box intVal; stringVal; doubleVal ]
    let genType = genInstance.GetType()
    argNames
    |> List.zip args
    |> List.iter (fun (expectedValue,name) ->
        let prop = genType.GetProperty name
        let genVal = prop.GetValue genInstance
        Assert.Equal (expectedValue, genVal))
    
    //validate the test1 arg
    let genTest1 = genType.GetProperty("arg").GetValue(genInstance)
    (genTest1 :?> Test1).CancellationTokenType <- CancellationToken.None
    (test1 :?> Test1).CancellationTokenType <- CancellationToken.None
    
    let a = Instance.indirectSerialize typeof<Test1> genTest1
    let b = Instance.indirectSerialize typeof<Test1> test1
    Assert.Equal(b,a)
