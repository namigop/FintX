namespace Tefin.Core

open System
open System.IO
open System.IO.Compression
open System.Runtime.InteropServices
open Tefin.Core.Interop

module Share =
    type ShareInfo =
        { Version: string
          Source: string
          CreatedAt: DateTime
          Type: string

        }

        static member ClientShare = "ClientShare"
        static member FileShare = "FileShare"
        static member FolderShare = "FolderShare"
        static member FileName = "Share.info"

    (* The folder structure
    projects
       - {projectName}
         - projectState.json
         - {clientName}
            - config.json
            - code
            - methods
                - {methodName}
                   - _autoSave
                   - file1.fxrq
                   - file2.fxrq
    *)

    let private createZip (io: IOResolver) targetZip (files: string array) (clientPath: string) (info:ShareInfo)=
        try
            if io.File.Exists targetZip then
                io.File.Delete targetZip

            use zip = io.Zip.Open targetZip ZipArchiveMode.Create

            files
            |> Array.iter (fun file ->
                let relativePath = file.Replace(clientPath, "")
                let entry = zip.CreateEntry relativePath
                use  writer = new StreamWriter(entry.Open())
                writer.Write (io.File.ReadAllText file)
                )

            let entry = zip.CreateEntry(ShareInfo.FileName)
            use  writer = new StreamWriter(entry.Open())
            writer.Write (Instance.jsonSerialize info)
                    
            Res.ok targetZip
        with exc ->
            Res.failed exc
    let createInfo shareType =
         { Version = Utils.appVersionSimple
                     Source = RuntimeInformation.RuntimeIdentifier
                     CreatedAt = DateTime.Now
                     Type = shareType
                      }

    let createFileShare (io: IOResolver) (targetZip: string) (files: string array) (clientPath: string) =
        let info = createInfo ShareInfo.FileShare
        createZip io targetZip files clientPath info

    let createFolderShare (folders: string array) (proj: Project) = ()

    let createClientShare (io: IOResolver) (targetZip: string) (clientPath: string) =
        let clientConfig = Path.Combine(clientPath, ClientGroup.ConfigFilename)

        if not (io.File.Exists clientConfig) then
            Res.failedWith $"{clientPath} is not a valid client path"
        else
            let filesToZip =
                io.Dir.GetFiles(clientPath, "*.*", SearchOption.AllDirectories)
                |> Array.filter (fun f -> not (Path.GetFileName(f) = ProjectSaveState.FileName))
                |> Array.filter (fun f ->
                    let parentDir = f |> Path.GetDirectoryName |> Path.GetFileName
                    not (parentDir = Project.autoSaveFolderName))

            let info = createInfo ShareInfo.ClientShare
            createZip io targetZip filesToZip clientPath info
//Res.failed ""
