namespace Tefin.Core

open System.IO
open System.IO.Compression
open System.Threading
open System.Threading.Tasks

type IFileIO =
    abstract Copy: source: string * target: string -> unit
    abstract Move: source: string * target: string -> unit
    abstract Copy: source: string * target: string * overwrite: bool -> unit
    abstract Delete: file: string -> unit
    abstract WriteAllBytesAsync: file: string -> bytes: byte array -> Task
    abstract WriteAllTextAsync: file: string -> content: string -> Task
    abstract WriteAllText: file: string -> content: string -> unit

    abstract ReadAllBytesAsync: file: string -> Task<byte array>
    abstract ExtractZip: zipFile: string -> path: string -> unit
    abstract Exists: file: string -> bool
    abstract CreateText: file: string -> StreamWriter
    abstract ReadAllText: file: string -> string
    abstract ReadAllLines: file: string -> string array

module File =
    let fileIO =
        { new IFileIO with
            member x.WriteAllTextAsync (file: string) (contents: string) = File.WriteAllTextAsync(file, contents)
            member x.WriteAllText (file: string) (contents: string) = File.WriteAllText(file, contents)
            member x.Copy(source: string, target: string) = File.Copy(source, target)
            member x.Move(source: string, target: string) = File.Move(source, target)
            member x.Copy(source: string, target: string, overwrite: bool) = File.Copy(source, target, overwrite)
            member x.Delete file = File.Delete file

            member x.WriteAllBytesAsync zip bytes =
                File.WriteAllBytesAsync(zip, bytes, CancellationToken.None)

            member x.ExtractZip zip path = ZipFile.ExtractToDirectory(zip, path)
            member x.Exists file = File.Exists file
            member x.CreateText file = File.CreateText file
            member x.ReadAllText file = File.ReadAllText file
            member x.ReadAllLines file = File.ReadAllLines file
            member x.ReadAllBytesAsync file = File.ReadAllBytesAsync file }
