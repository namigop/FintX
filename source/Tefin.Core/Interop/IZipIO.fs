namespace Tefin.Core

open System.IO.Compression
open System.IO

type IZipIO =
  abstract Open: zipFile: string -> mode: ZipArchiveMode -> ZipArchive
  abstract OpenRead: zipFile: string -> ZipArchive
  abstract ExtractToDirectory: zipFile: string -> targetDir:string -> overwrite: bool -> unit
  
module Zip =
  let zipIO =
    { new IZipIO with
        member x.Open zip mode = ZipFile.Open(zip, mode)
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
              ignore(Directory.CreateDirectory dir)
              if (File.Exists target) then

                let fileStart = Path.GetFileNameWithoutExtension target
                let ext = Path.GetExtension target
                let newTarget = 
                  Utils.getAvailableFileName dir fileStart ext
                  |> fun n -> Path.Combine(dir, n)
                entry.ExtractToFile newTarget
              else
                
                entry.ExtractToFile target
             )
           (*
           foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    // Gets the full path to ensure that relative segments are removed.
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                    // are case-insensitive.
                    if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        entry.ExtractToFile(destinationPath);
                }
            }
           *)
           ()
        }
