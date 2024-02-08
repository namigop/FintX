module Tefin.Core.Log

open System

type LogType =
  | Info
  | Warn
  | Debug
  | Error

type ILog =
  abstract Info: string -> unit
  abstract Warn: string -> unit
  abstract Error: string -> unit
  abstract Error: Exception -> unit
  abstract Debug: string -> unit

let private write (msg: string) = Console.WriteLine msg

let private write2 (msg: string) =
      #if DEBUG
      Console.WriteLine msg
      #endif
      #if !DEBUG
      ()
      #endif


let private log (l: LogType) (msg:string) =
  match l with
  | LogType.Info -> write $"INFO: {msg}"
  | LogType.Warn -> write $"WARN: {msg}"
  | LogType.Error -> write $"ERROR: {msg}"
  | LogType.Debug -> write2 $"DEBUG: {msg}"
 
let logInfo = log LogType.Info
let logError = log LogType.Error
let logExc (exc: Exception) = exc.ToString() |> logError
let logWarn = log LogType.Warn
let logDebug = log LogType.Debug
