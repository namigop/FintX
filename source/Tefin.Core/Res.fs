namespace Tefin.Core
open System

open System
open System.Runtime.InteropServices.JavaScript
open System.Threading.Tasks

type public Ret<'a> = Result<'a, Exception>
module Res =

    let ok v = Ret.Ok v
    let failed (exc: Exception) = Ret.Error exc

    let failedWith (err: string) = failed <| Exception(err)

    let isOk (res: Ret<'a>)  =
        match res with
        | Ret.Ok _ -> true
        | Ret.Error _ -> false
    let getError (res: Ret<'a>)  =
        match res with
        | Ret.Ok _ -> Unchecked.defaultof<Exception>
        | Ret.Error err -> err
    let getValue (res: Ret<'a>)  =
        match res with
        | Ret.Ok v -> v
        | Ret.Error _ -> Unchecked.defaultof<'a>

    let bind fn  (res: Ret<'a>)  =
       try
           match res with
           | Ret.Ok v -> fn v
           | Ret.Error _ -> res
       with
        | exc -> failed exc

    let exec fn =
        try
            fn() |> ok
        with
        | exc -> failed exc

    let execTask (fn: unit -> Task<'a>) = task {
        try
            let! v = fn()
            return (ok v)
        with
        | exc -> return (failed exc)
    }
    let execWithArg fn arg =
        try
            arg |> fn |> ok
        with
        | exc -> failed exc

    let intercept onOk onFailed (res: Ret<'a>) =
        try
            match res with
            | Ret.Ok v -> onOk v; res
            | Ret.Error err -> onFailed err; res
        with
        | exc -> failed exc

    let mapErr mapFn (res: Ret<'a>) =
        try
            match res with
            | Ret.Ok v -> ok v
            | Ret.Error exc -> exc |> mapFn |> ok
        with
        | exc -> failed exc

    let map mapFn (res: Ret<'a>) =
        try
            match res with
            | Ret.Ok v -> v |> mapFn |> ok
            | Ret.Error err -> failed err
        with
        | exc -> failed exc

    let mapTask (mapFn: 'a -> Task<Ret<'b>>) (res: Task<Ret<'a>>) = task {
        try
            let! r = res
            match r with
            | Ret.Ok v ->
                let! mappedValue = mapFn v
                return mappedValue
            | Ret.Error err -> return (failed err)
        with
        | exc -> return (failed exc)
     }

    let stop mapFn (res: Ret<'a>) = ignore (map mapFn res)

    let log = intercept