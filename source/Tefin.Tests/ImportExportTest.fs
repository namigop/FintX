module ImportExportTest


open System
open System.Threading
open Grpc.Core
open Newtonsoft.Json.Linq
open Tefin.Core
open Tefin.Grpc
open Tefin.Grpc.Dynamic
open Tefin.Views.Types
open Xunit
open Tefin.Tests.TestInputTypes

 
//An approximation of the gRPC AsyncClientStreaming class
type AsyncClientStreaming<'T>()=
  let mutable stream =Array.empty<'T>
  member x.RequestStream
    with get() = stream
    and set v = stream <-v
    
//An approximation of the gRPC AsyncDuplexStreaming class
type AsyncDuplexStreaming<'TReq, 'TResp>()=
  let mutable stream =Array.empty<'TReq>
  let mutable stream2 =Array.empty<'TResp>
  member x.RequestStream
    with get() = stream
    and set v = stream <-v
  member x.ResponseStream
    with get() = stream2
    and set v = stream2 <-v



type Sample() =    
    member x.UnaryTest(arg:Test1, intArg:int, stringArg:string, doubleArg:float) =
        arg
    member x.ClientStreamTest(headers:Metadata, deadline:Nullable<DateTime>, cancellationToken:CancellationToken) =
      AsyncClientStreaming<Test1>()
    member x.DuplexStreamTest(headers:Metadata, deadline:Nullable<DateTime>, cancellationToken:CancellationToken) =
      AsyncDuplexStreaming<Test1, string>() 
  
[<Fact>]
let rec ``Can export gRPC DuplexStream`` () =
    let mi = typeof<Sample>.GetMethod("DuplexStreamTest")
    let struct (_,test1) = buildType typeof<Test1>
    typeof<Test1>.GetProperty("IntType").SetValue(test1, 42);
    let struct (_,test2) = buildType typeof<Test1>
    let clientStream = [| test1; test2 |]
    
    let headers = Metadata()
    headers.Add("key", "value")
    headers.Add("key2", "value2")
    let methodParams = [| (box <| headers); DateTime.MinValue ; CancellationToken.None  |]
    let serParam = { Method = mi; RequestParams = methodParams ; EnvVariables= AllVariables.Empty(); RequestStream = Some clientStream }
    let jsonRet = Export.requestToJson(serParam)
    Assert.True(Res.isOk jsonRet)
    
    let testData =
      [ ("$.Api", GrpcPackage.packageName)
        ("$.Method", "DuplexStreamTest")
        ("$.MethodType", $"{MethodType.Duplex}")
        ("$.RequestType", typeof<Test1>.FullName)   ] //For a gRPC Client stream, the requestType
                                                                  //is the type of a stream item
    let jObj = JObject.Parse (Res.getValue jsonRet)
    for (jPath, expected) in testData do
      let argToken = jObj.SelectToken jPath
      Assert.False argToken.HasValues
      let api = argToken.ToObject()
      Assert.Equal(expected, api)
    
    //Validate the request parameters  
    let reqToken = jObj.SelectToken "$.Request"
    Assert.True reqToken.HasValues
    let headersToken = jObj.SelectToken "$.Request.headers" :?> JArray
    Assert.Equal(2, headersToken.Count) //we have 2 metadata.entry instances
    let argToken = jObj.SelectToken "$.Request.deadline"
    Assert.False argToken.HasValues
    let argToken = jObj.SelectToken "$.Request.cancellationToken"
    Assert.True argToken.HasValues
            
    //For a client stream, the RequestStream prop should be populated
    let jTest1 = jObj.SelectToken "$.RequestStream[0]"
    let jTest2 = jObj.SelectToken "$.RequestStream[1]"
    Assert.True(jTest1.HasValues)
    Assert.True(jTest2.HasValues)
    
    //validate that the json in the export file is matching our Test1 input instance
    let fromExport = jTest1.ToObject<Test1>()
                     |> fun t -> t.CancellationTokenType <- CancellationToken.None; t
                     |> fun i ->  Instance.jsonSerialize<Test1>(i)
    (test1 :?> Test1).CancellationTokenType <- CancellationToken.None
    let fromInput = Instance.jsonSerialize<Test1>(test1 :?> Test1)
    Assert.Equal(fromInput, fromExport)
  
  
[<Fact>]
let ``Can export gRPC ClientStream`` () =
    let mi = typeof<Sample>.GetMethod("ClientStreamTest")
    let struct (_,test1) = buildType typeof<Test1>
    typeof<Test1>.GetProperty("IntType").SetValue(test1, 42);
    let struct (_,test2) = buildType typeof<Test1>
    let clientStream = [| test1; test2 |]
    let methodParams = [| (box <| Metadata()); null; CancellationToken.None  |]
    let serParam = { Method = mi; RequestParams = methodParams ; EnvVariables= AllVariables.Empty(); RequestStream = Some clientStream }
    let jsonRet = Export.requestToJson(serParam)
    Assert.True(Res.isOk jsonRet)
    
    let testData =
      [ ("$.Api", GrpcPackage.packageName)
        ("$.Method", "ClientStreamTest")
        ("$.MethodType", $"{MethodType.ClientStreaming}")
        ("$.RequestType", typeof<Test1>.FullName)   ] //For a gRPC Client stream, the requestType
                                                                  //is the type of a stream item
    let jObj = JObject.Parse (Res.getValue jsonRet)
    for (jPath, expected) in testData do
      let argToken = jObj.SelectToken jPath
      Assert.False argToken.HasValues
      let api = argToken.ToObject()
      Assert.Equal(expected, api)
    
    //Validate the request parameters  
    let reqToken = jObj.SelectToken "$.Request"
    Assert.True reqToken.HasValues
    let argToken = jObj.SelectToken "$.Request.headers"
    Assert.False argToken.HasValues
    let argToken = jObj.SelectToken "$.Request.deadline"
    Assert.False argToken.HasValues
    let argToken = jObj.SelectToken "$.Request.cancellationToken"
    Assert.True argToken.HasValues
            
    //For a client stream, the RequestStream prop should be populated
    let jTest1 = jObj.SelectToken "$.RequestStream[0]"
    let jTest2 = jObj.SelectToken "$.RequestStream[1]"
    Assert.True(jTest1.HasValues)
    Assert.True(jTest2.HasValues)
    
    //validate that the json in the export file is matching our Test1 input instance
    let fromExport = jTest1.ToObject<Test1>() 
                     |> fun t -> t.CancellationTokenType <- CancellationToken.None; t
                     |> fun i ->  Instance.jsonSerialize<Test1>(i)
    (test1 :?> Test1).CancellationTokenType <- CancellationToken.None
    
    
    let fromInput = Instance.jsonSerialize<Test1>(test1 :?> Test1)
    Assert.Equal(fromInput, fromExport)
  
[<Fact>]
let ``Can export gRPC UnaryAndServerStream`` () = //Both unary and server streaming have 1 request
    let mi = typeof<Sample>.GetMethod("UnaryTest")
    let struct (ok,test1) = buildType typeof<Test1>
    let intVal = 42
    let stringVal = "foobar"
    let doubleVal = 42.42
    
    let methodParams = [| box test1; intVal; stringVal; doubleVal |]
    let serParam = { Method = mi; RequestParams = methodParams ;EnvVariables= AllVariables.Empty(); RequestStream = None }
    let jsonRet = Export.requestToJson(serParam)
    Assert.True(Res.isOk jsonRet)
    
    let jObj = JObject.Parse (Res.getValue jsonRet)
    let testData =
      [ ("$.Api", GrpcPackage.packageName)
        ("$.Method", "UnaryTest")
        ("$.MethodType", "Unary")
        ("$.RequestType", "Tefin.Tests.TestInputTypes+Test1")   ] //For a gRPC Unary, the first parameter of a method is the request
    
    for (jPath, expected) in testData do
      let argToken = jObj.SelectToken jPath
      Assert.False argToken.HasValues
      let api = argToken.ToObject()
      Assert.Equal(expected, api)
    
    
    let reqToken = jObj.SelectToken "$.Request"
    Assert.True reqToken.HasValues
    let argToken = jObj.SelectToken "$.Request.arg"
    Assert.True argToken.HasValues
    
    
    (* Expected Export Json (for a unary method)
{
  "Api": "grpc",
  "Method": "RunTest1",
  "MethodType": "Unary",
  "RequestType": "Tefin.Tests.TestInputTypes+Test1",
  "Request": {
    "arg": {
      "IntType": 0,
      "Int16Type": 0,
      "Int64Type": 0,
      "DoubleType": 0.0,
      "DecimalType": 0.0,
      "SingleType": 0.0,
      "UIntType": 0,
      "UInt16Type": 0,
      "UInt64Type": 0,
      "BoolType": true,
      "DateTimeType": "2023-12-14T21:29:41.733552+08:00",
      "DateTimeOffsetType": "2023-12-14T21:29:41.733716+08:00",
      "GuidType": "f51ea2d5-8b26-428d-8957-0147b192bce7",
      "TimeSpanType": "00:00:01",
      "CancellationTokenType": {
        "IsCancellationRequested": false,
        "CanBeCanceled": false,
        "WaitHandle": {
          "Handle": {
            "value": 944
          },
          "SafeWaitHandle": {
            "IsInvalid": false,
            "IsClosed": false
          }
        }
      },
      "UriType": "http://localhost:8080/",
      "CharType": "c",
      "ByteType": 0,
      "StringType": ""
    },
    "intArg": 42,
    "stringArg": "foobar",
    "doubleArg": 42.42
  }
}
    
    *)
    