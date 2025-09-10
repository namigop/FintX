module Tefin.Tests.ScriptingTests
open System
open Tefin.Core
open Xunit

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
          