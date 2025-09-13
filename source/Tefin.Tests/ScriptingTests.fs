module Tefin.Tests.ScriptingTests
open System
open Tefin.Core.Scripting
open Tefin.Core
open Xunit


[<Fact>]
let ``Can parse script with raw text`` () =
    let script =
        """
           {
            "Api": "grpc",
            "Method": "ReportScenario",
            "MethodType": "Unary",
            "ClientType": "Grpc.Testing.ReportQpsScenarioService+ReportQpsScenarioServiceClient",
            "RequestType": "Grpc.Testing.ScenarioResult",
            "Variables": {
                "RequestVariables": [],
                "ResponseVariables": [],
                "RequestStreamVariables": [],
                "ResponseStreamVariables": []
            }
          }
        """
    let res = ScriptParser.parse script "" ""
    Assert.Equal(true, res.IsOk)
    let lines = Res.getValue res
    Assert.Equal(15, lines.Length)
    for l in lines do
      Assert.Equal(l.LineStart, l.LineEnd)
      Assert.Equal(false, l.IsComment)    
   

[<Fact>]
let ``Can parse script with raw one liners`` () =
    let script =
        """
           {
            "Api": "$<<DateTine.Now.ToString()>>",
            "Method": "ReportScenario",
            "MethodType": "Unary",
            "ClientType": "Grpc.Testing.ReportQpsScenarioService+ReportQpsScenarioServiceClient",
            "RequestType": "$<<return 42;>>",
            "Variables": {
                "RequestVariables": [],
                "ResponseVariables": [],
                "RequestStreamVariables": [],
                "ResponseStreamVariables": []
            }
          }
        """
    let res = ScriptParser.parse script "" ""
    Assert.Equal(true, res.IsOk)
    let lines = Res.getValue res
    Assert.Equal(15, lines.Length)
    for l in lines do
      Assert.Equal(l.LineStart, l.LineEnd)
      Assert.Equal(false, l.IsComment)
    
    let oneliners = lines |> Array.filter (fun l -> l.ContainsScript)
    Assert.Equal(2, oneliners.Length)
    Assert.True(oneliners[0].Raw.Contains("$<<DateTine.Now.ToString()>>"))
    Assert.True(oneliners[1].Raw.Contains("$<<return 42;>>"))



[<Fact>]
let ``Can parse script with raw multi liners`` () =
    let script =
        """
           {
            "Api": "$<<DateTine.Now.ToString()>>",
            "Method": "ReportScenario",
            "MethodType": "Unary",
            "ClientType": "Grpc.Testing.ReportQpsScenarioService+ReportQpsScenarioServiceClient",
            "RequestType": "$<<
               int GetAnswerToTheQuestionOfLife() {
                    return 42;
               }
               return GetAnswerToTheQuestionOfLife();
               >>",
            "Variables": {
                "RequestVariables": []                  
                "ResponseVariables": [],
                "RequestStreamVariables": [],
                "ResponseStreamVariables": []
            }
          }
        """
    let res = ScriptParser.parse script "" ""
    Assert.Equal(true, res.IsOk)
    let lines = Res.getValue res
    Assert.Equal(15, lines.Length)
    for l in lines do
      Assert.Equal(l.LineStart, l.LineEnd)
      Assert.Equal(false, l.IsComment)
    
    let oneliners = lines |> Array.filter (fun l -> l.ContainsScript)
    Assert.Equal(2, oneliners.Length)
    Assert.True(oneliners[0].Raw.Contains("$<<DateTine.Now.ToString()>>"))
    Assert.True(oneliners[1].Raw.Contains("$<<return 42;>>"))



[<Fact>]
let ``Can execute script`` () =
    task {
        let cs = "2 + 3"
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal("5", Res.getValue res)
    }

[<Fact>]
let ``Can execute multiple scripts`` () =
    task {
        let cs1 = "2 + 3"
        let cs2 =  """ DateTime.Today.ToString("yyyy-MM-dd HHmmss") """
        let today =  DateTime.Today.ToString("yyyy-MM-dd HHmmss")
        
        let engine = Script.createEngine "someId"
        let! res1 = Script.run engine cs1
        let! res2 = Script.run engine cs2
        Assert.Equal("5", Res.getValue res1)
        Assert.Equal(today, Res.getValue res2)
        
        Assert.Equal(2, engine.Runners.Count)
        Assert.Equal("someId", engine.Id)
    }

[<Fact>]
let ``Can execute handle null return`` () =
    task {
        let cs = "return null;"
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal("<null>", Res.getValue res)
    }
        
[<Fact>]
let ``Can execute script with function`` () =
    task {
        let cs =
            """
                string Get() {
                   return "ABCDE".ToLower();
                }
                
                return Get();   
            """
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal("abcde", Res.getValue res)
    }
    
[<Fact>]
let ``Can handle exception`` () =
    task {
        let cs =
            """
                string Get() {
                   throw new Exception("Some script exception");
                }
                
                return Get();   
            """
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal(true, res.IsError)
        let err = Res.getError res
        Assert.Equal("Some script exception", err.Message)
    }
    
[<Fact>]
let ``Can run linq`` () =
    task {
        let cs =
            """
               Enumerable.Range(1, 10)
               .Where(x => x % 2 == 0)
               .Select(t => t * 2)
               .Sum()            
            """
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal( (2 * (2 + 4 + 6 + 8 + 10)).ToString() , Res.getValue res)        
    }
    
[<Fact>]
let ``Can execute async script`` () =
    task {
        let cs =
            """
                async Task<string> Get() {
                   return "ABCDE".ToLower();
                }
                
                return Get();   
            """
        let engine = Script.createEngine "someId"
        let! res = Script.run engine cs
        Assert.Equal("abcde", Res.getValue res)
    }
          