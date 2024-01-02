namespace Tefin.Grpc.Execution
open System

type CallError() =
        let mutable e = Unchecked.defaultof<Exception>
        member x.Exception
            with get() = e
        member x.Receive (exc) = e <- exc
        member x.Failed with get() = not(e = null)

