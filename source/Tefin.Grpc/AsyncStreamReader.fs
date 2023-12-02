namespace Tefin.Grpc.Custom

open System
open System.Threading
open System.Threading.Tasks
open Grpc.Core
open Tefin.Core

type AsyncStreamReader<'T>(stream: IAsyncStreamReader<'T>, onMoveNext: IAsyncStreamReader<'T> -> CancellationToken -> Task<bool>) =
    member this.MoveNext(cancellationToken) =
        task {
            let! b = onMoveNext stream cancellationToken
            return b
        }

    member this.Current = stream.Current

    interface IAsyncStreamReader<'T> with
        member this.MoveNext(cancellationToken) = this.MoveNext cancellationToken
        member this.Current = this.Current

module TimedAsyncStreamReader =
    let create<'T> (io: IOResolver) (stream: IAsyncStreamReader<'T>) (clientName:string) (method:string) =
        let onSuccess retValue (ts: TimeSpan) =
            io.Log.Info $"Call to MoveNext = {retValue}. Elapsed {ts.TotalMilliseconds} msec"
            io.MethodCall.Publish(clientName, method, ts.TotalMilliseconds);

        let onError (exc: Exception) (ts: TimeSpan) =
            io.Log.Error $"Call to MoveNext failed. Elapsed {ts.TotalMilliseconds} msec"
            io.Log.Error $"{exc}"

        let onMoveNext (reader: IAsyncStreamReader<'T>) (token: CancellationToken) =
            task {
                let! (ok, ts) = TimeIt.runTaskWithReturnValue (fun () -> reader.MoveNext token) (Some onSuccess) (Some onError)
                return ok
            }

        new AsyncStreamReader<'T>(stream, onMoveNext)