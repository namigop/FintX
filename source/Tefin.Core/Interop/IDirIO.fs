namespace Tefin.Core

open System.IO

type IDirIO =
  abstract GetFiles: path: string -> string array
  abstract GetFiles: path: string * searchPattern: string -> string array
  abstract GetFiles: path: string * searchPattern: string * searchOption: SearchOption -> string array
  abstract CreateDirectory: path: string -> unit
  abstract SetCurrentDirectory: path: string -> unit
  abstract Exists: path: string -> bool
  abstract GetDirectories: path: string -> string array
  abstract Delete: path: string -> recursive: bool -> unit
  abstract Move: oldPath: string -> newPath: string -> unit

module Dir =
  let dirIO =
    { new IDirIO with

        member x.Move oldPath newPath = Directory.Move(oldPath, newPath)
        member x.GetFiles(path: string, searchPattern: string) = Directory.GetFiles(path, searchPattern)
        member x.GetDirectories path = Directory.GetDirectories path

        member x.Delete path (recursive: bool) = Directory.Delete(path, recursive)

        member x.GetFiles(path: string, searchPattern: string, opt: SearchOption) =
          Directory.GetFiles(path, searchPattern, opt)

        member x.GetFiles(path: string) = Directory.GetFiles(path)
        member x.CreateDirectory path = ignore (Directory.CreateDirectory path)
        member x.SetCurrentDirectory path = Directory.SetCurrentDirectory path
        member x.Exists path = Directory.Exists path }
