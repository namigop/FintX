module Tefin.Core.TimeIt

open System
open System.Diagnostics
open System.Threading.Tasks

let private handleError exc (sw: Stopwatch) (onError: (Exception -> TimeSpan -> unit) option) =
  sw.Stop()

  if onError.IsSome then
    onError.Value exc sw.Elapsed

  ()
//sw.Elapsed

// raise exc
let runAction
  (action: unit -> unit)
  (onSuccess: (TimeSpan -> unit) option)
  (onError: (Exception -> TimeSpan -> unit) option)
  =
  let sw = Stopwatch.StartNew()

  try
    action ()
    sw.Stop()

    if onSuccess.IsSome then
      onSuccess.Value sw.Elapsed

    sw.Elapsed
  with exc ->
    handleError exc sw onError
    sw.Stop()
    sw.Elapsed

let runActionWithReturnValue
  (action: unit -> 'T)
  (onSuccess: ('T -> TimeSpan -> unit) option)
  (onError: (Exception -> TimeSpan -> unit) option)
  =
  let sw = Stopwatch.StartNew()

  try
    let ret = action ()
    sw.Stop()

    if onSuccess.IsSome then
      onSuccess.Value ret sw.Elapsed

    struct (ret, sw.Elapsed)
  with exc ->
    handleError exc sw onError
    sw.Stop()
    struct (Unchecked.defaultof<'T>, sw.Elapsed)

let runTask
  (action: unit -> Task)
  (onSuccess: (TimeSpan -> unit) option)
  (onError: (Exception -> TimeSpan -> unit) option)
  =
  task {
    let sw = Stopwatch.StartNew()

    try
      do! action ()
      sw.Stop()

      if onSuccess.IsSome then
        onSuccess.Value sw.Elapsed

      return sw.Elapsed
    with exc ->
      sw.Stop()
      handleError exc sw onError
      return sw.Elapsed
  }

let runTaskWithReturnValue<'T>
  (action: unit -> Task<'T>)
  (onSuccess: ('T -> TimeSpan -> unit) option)
  (onError: (Exception -> TimeSpan -> unit) option)
  =
  task {
    let sw = Stopwatch.StartNew()

    try
      let! r = action ()
      sw.Stop()

      if onSuccess.IsSome then
        onSuccess.Value r sw.Elapsed

      return (r, sw.Elapsed)
    with exc ->
      sw.Stop()
      handleError exc sw onError
      return (Unchecked.defaultof<'T>, sw.Elapsed)
  // let ret =
  //     action()
  //     |> Async.AwaitTask
  //     |> Async.Catch
  //     |> Async.RunSynchronously
  //     |> function
  //          | Choice1Of2 r ->
  //              sw.Stop()
  //              if onSuccess.IsSome then
  //                 onSuccess.Value r sw.Elapsed
  //              struct (r, sw.Elapsed)
  //          | Choice2Of2 exc -> handleError exc sw onError
  // return ret
  }
