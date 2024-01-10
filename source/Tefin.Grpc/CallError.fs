namespace Tefin.Grpc.Execution
open System

type CallError() =
        let mutable err = Unchecked.defaultof<Exception>
        member x.Exception
            with get() = err
        member x.Receive (exc) = err <- exc
        member x.Failed with get() = not(err = null)
        member x.Clear() = err <- null

