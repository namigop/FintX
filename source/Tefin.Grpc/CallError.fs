namespace Tefin.Grpc.Execution

open System

type CallError() =
  let mutable err = Unchecked.defaultof<Exception>
  member x.Exception = err
  member x.Receive(exc) = err <- exc
  member x.Failed = not (err = null)
  member x.Clear() = err <- null
