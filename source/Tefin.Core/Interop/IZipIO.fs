namespace Tefin.Core

open System
open System.IO.Compression
open System.IO

type IZipEntry =
  abstract Open: unit -> Stream

type IZipArchive =
  inherit IDisposable
  abstract CreateEntry: path: string -> IZipEntry

type IZipIO =
  abstract Open: zipFile: string -> mode: ZipArchiveMode -> IZipArchive
  abstract OpenRead: zipFile: string -> ZipArchive
  abstract ExtractToDirectory: zipFile: string -> targetDir: string -> overwrite: bool -> unit

module Zip =
  let zipIO =
    let createEntry (z: ZipArchiveEntry) =
      { new IZipEntry with
          member x.Open() = z.Open() }

    let createZip (z: ZipArchive) =
      { new IZipArchive with
          member x.CreateEntry path = z.CreateEntry path |> createEntry
          member x.Dispose() = z.Dispose() }

    { new IZipIO with
        member x.Open zip mode = ZipFile.Open(zip, mode) |> createZip
        member x.OpenRead zipFile = ZipFile.OpenRead zipFile
        member x.ExtractToDirectory zipFile targetDir overwrite =
          if overwrite then
            ZipFile.ExtractToDirectory(zipFile, targetDir, true)
          else
            use zipArchive = x.OpenRead zipFile

            zipArchive.Entries
            |> Seq.iter (fun entry ->
              let target = Path.Combine(targetDir, entry.FullName) |> Path.GetFullPath
              let dir = Path.GetDirectoryName target
              ignore (Directory.CreateDirectory dir)

              if (File.Exists target) then

                let fileStart = Path.GetFileNameWithoutExtension target
                let ext = Path.GetExtension target

                let newTarget =
                  Utils.getAvailableFileName dir fileStart ext |> fun n -> Path.Combine(dir, n)

                entry.ExtractToFile newTarget
              else
                entry.ExtractToFile target)
           
             }
