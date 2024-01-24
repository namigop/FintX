namespace Tefin.Core.Interop

open System

type ErrorResponse() =
    member val Error = "" with get, set

    interface IDisposable with
        member x.Dispose() = ()
//member val StackTrace = "" with get,set
