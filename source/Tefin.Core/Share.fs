namespace Tefin.Core

open System
open System.IO
open System.IO.Compression
open Tefin.Core.Interop

module Share =
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
    
    let private createZip  (io:IOResolver) targetZip  (files: string array) (projPath:string) =
      try
        if io.File.Exists targetZip then
          io.File.Delete targetZip
          
        use zip = io.Zip.Open targetZip ZipArchiveMode.Create      
        files
        |> Array.iter (fun file ->
          let relativePath = file.Replace(projPath, "")  
          ignore(zip.CreateEntry relativePath))
        Res.ok targetZip
      with exc ->
        Res.failed exc
    let createFileShare (io:IOResolver) (targetZip:string) (files: string array) (projPath:string) =   
      createZip io targetZip files projPath
      
    let createFolderShare(folders:string array) (proj:Project) =
      ()
    let createClientShare (io:IOResolver) (targetZip:string) (clientPath:string)  =   
      let clientConfig = Path.Combine(clientPath, ClientGroup.ConfigFilename)
      if not (io.File.Exists clientConfig) then
        Res.failedWith $"{clientPath} is not a valid client path"
      else
        let filesToZip =
          io.Dir.GetFiles(clientPath, "*.*", SearchOption.AllDirectories)
          |> Array.filter (fun f -> not(Path.GetFileName(f) = ProjectSaveState.FileName))
          |> Array.filter (fun f ->
              let parentDir = f |> Path.GetDirectoryName |> Path.GetFileName
              not (parentDir = Project.autoSaveFolderName)
              )
        createZip io targetZip filesToZip clientPath
        //Res.failed ""
        
        
        

