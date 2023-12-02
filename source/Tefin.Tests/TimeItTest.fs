module Tefin.Tests.TimeItTest


open System
open System.Threading
open System.Threading.Tasks
open Tefin.Core
open Tefin.Tests.TestInputTypes
open Tefin.ViewModels.Types
open Tefin.ViewModels.Types.TypeNodeBuilders
open Xunit
 
    
[<Fact>]
let ``Can time async functions with return value``() = task {
     let foo() = task{
          do! Async.Sleep(1000) |> Async.StartAsTask
          return 100
     }
     
     let onSuccess v (ts:TimeSpan) =
          Assert.Equal(100, v)
          Assert.True(ts.TotalMilliseconds >= 1000)   
          
     let! (v,ts) = TimeIt.runTaskWithReturnValue foo (Some onSuccess) None
     
     Assert.Equal(100, v)
     Assert.True(ts.TotalMilliseconds >= 1000)      
}

[<Fact>]
let ``Can time async functions``() = task {
     let foo() = Task.Delay(1000)
     let! ts = TimeIt.runTask foo None None
     Assert.True(ts.TotalMilliseconds >= 1000)      
}
      
[<Fact>]
let ``Can time functions``() = task {
     let foo() = Thread.Sleep(1000)
     let ts = TimeIt.runAction foo None None
     Assert.True(ts.TotalMilliseconds >= 1000)      
}
    
[<Fact>]
let ``Can handle error in functions``() = task {
     let err = "error message"
     let foo() = failwith err
     let onError (exc:Exception) ts =
          Assert.Equal(err, exc.Message)

     try
          let struct (v, ts) = TimeIt.runActionWithReturnValue foo None (Some onError)
          ()
     with
     | exc ->
          Assert.Equal(err, exc.Message)
}
    