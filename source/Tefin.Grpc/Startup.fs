namespace Tefin.Grpc

open Tefin.Core
open Tefin.Core.Reflection
open System.Threading.Tasks
module Startup =

    let init() = task {
        TypeBuilder.register GrpcTypeBuilder.getDefault
        TaskScheduler.UnobservedTaskException
        |> Observable.add (fun arg ->            
            arg.SetObserved()
            Resolver.value.Log.Warn (arg.Exception.ToString())
            ())
    }