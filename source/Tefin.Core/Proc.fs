module Tefin.Core.Proc

open System
open System.Threading.Tasks
open System.Diagnostics
open Tefin.Core.Log

let run<'a> (exe:string) (args:string) (onExit: unit -> 'a) =
    let tcs = new TaskCompletionSource<'a>()
    let proc = new Process()
    proc.EnableRaisingEvents <- true
    proc.StartInfo.FileName <- exe
    proc.StartInfo.Arguments <- args
    proc.StartInfo.UseShellExecute <- false
    proc.StartInfo.CreateNoWindow <- true

    proc.StartInfo.RedirectStandardOutput <- true
    proc.StartInfo.RedirectStandardError <- true
    proc.OutputDataReceived
    |> Observable.add  (fun d ->
        if not(d = null) && not(d.Data = null) then
            Console.WriteLine d.Data)

    proc.ErrorDataReceived
    |> Observable.add  (fun d ->
        if not(d = null) && not(d.Data = null) then
            if not (tcs.Task.IsCompleted) then
                tcs.SetException(Exception(d.Data)) |> ignore
            Console.WriteLine d.Data)

    proc.Exited
    |> Observable.add (fun args ->
            logInfo $"Process done: {exe}"
            if not (tcs.Task.IsCompleted) then
                let files = onExit()
                tcs.SetResult files
            //tcs.TrySetResult(files)
        )

    logInfo ($"Executing: {exe} {args}")
    let ok = proc.Start()
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()
    tcs.Task