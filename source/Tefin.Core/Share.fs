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
      ClientName: string }
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

  let private createZip (io: IOResolver) targetZip (files: string array) (clientPath: string) (info: ShareInfo) =
    try
      if io.File.Exists targetZip then
        io.File.Delete targetZip

      use zip = io.Zip.Open targetZip ZipArchiveMode.Create

      files
      |> Array.iter (fun file ->
        let projPath = Path.GetDirectoryName clientPath
        let relativePath = file.Replace(projPath, "") |> fun c ->
            if c.StartsWith Path.DirectorySeparatorChar then
              c.TrimStart(Path.DirectorySeparatorChar)
            else
              c

        let entry = zip.CreateEntry relativePath
        use writer = new StreamWriter(entry.Open())
        writer.Write(io.File.ReadAllText file))

      let entry = zip.CreateEntry(ShareInfo.FileName)
      use writer = new StreamWriter(entry.Open())
      writer.Write(Instance.jsonSerialize info)

      Res.ok targetZip
    with exc ->
      Res.failed exc

  let private getClientFiles (io:IOResolver) (client:ClientGroup)  =
    let clientPath = client.Path
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
      Res.ok filesToZip
      
  let createInfo clientName shareType =
    { Version = Utils.appVersionSimple
      Source = RuntimeInformation.RuntimeIdentifier
      CreatedAt = DateTime.Now
      ClientName = clientName
      Type = shareType }

  
  let createFileShare (io: IOResolver) (targetZip: string) (files: string array) (client: ClientGroup) =
    let methodsPath = Project.getMethodsPath client.Path
    let filterOutMethodFiles (files:string array) = seq {
      for file in files do
          if not (file.StartsWith methodsPath) then
            yield file
    }
     
    (getClientFiles io client)
    |> Res.map (fun allFiles ->
        (filterOutMethodFiles allFiles)
        |> Seq.append files
        |> Seq.toArray
      )
    |> Res.map (fun targetFiles ->
        let info = createInfo client.Name ShareInfo.FileShare
        createZip io targetZip targetFiles client.Path info)
    |> Res.getValue
      
  let createFolderShare (io: IOResolver) (targetZip: string) (methodName: string) (client: ClientGroup) =    
    let methodPath = Project.getMethodPath client.Path methodName
    let methodsPath = Project.getMethodsPath client.Path
    
    let filterForMethodFiles (files:string array) = seq {
      for file in files do
          if (file.StartsWith methodsPath) then
            if (file.StartsWith methodPath) then
              yield file
          else
            yield file
    }
    getClientFiles io client
    |> Res.map (fun files -> (filterForMethodFiles files) |> Seq.toArray) 
    |> Res.map (fun files ->
         let info = createInfo client.Name ShareInfo.FileShare
         createZip io targetZip files client.Path info)
    |> Res.getValue

  let createClientShare (io: IOResolver) (targetZip: string) (client: ClientGroup) =
    let clientPath = client.Path
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

      let info = createInfo client.Name ShareInfo.ClientShare
      createZip io targetZip filesToZip clientPath info

  let importInto (io: IOResolver) (project: Project) (zip: string) =
    let allowMultiple ext =
      let extensions = [| $"{Ext.requestFileExt}" |]
      extensions |> Array.tryFind (fun c -> c = ext) |> Option.isSome

    use zipArchive = io.Zip.OpenRead zip

    let info = zipArchive.Entries |> Seq.find (fun entry -> entry.Name = ShareInfo.FileName)

    use reader = new StreamReader(info.Open())
    let i = reader.ReadToEnd() |> Instance.jsonDeserialize<ShareInfo>
    let clientName = i.ClientName
    let clientOpt = project.Clients |> Array.tryFind (fun c -> c.Name = clientName)
    let mutable updated = false

    match clientOpt with
    | Some(client) ->
      let clientPath = client.Path

      for entry: ZipArchiveEntry in zipArchive.Entries do
        let target = Path.Combine(project.Path, entry.FullName) |> Path.GetFullPath
        let dir = Path.GetDirectoryName target
        io.Dir.CreateDirectory dir
        let ext = Path.GetExtension target

        if (io.File.Exists target) then
          if (allowMultiple ext) then
            let fileStart = Path.GetFileNameWithoutExtension target
            let newTarget = Utils.getAvailableFileName dir fileStart ext |> fun n -> Path.Combine(dir, n)
            
            updated <- true
            entry.ExtractToFile(newTarget)
        else
          updated <- true
          entry.ExtractToFile(target)
    | None ->
      //extract away      
      io.Zip.ExtractToDirectory zip project.Path false
      updated <- true

    //return
    clientName, updated
