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

  let private createZip
    targetZip
    (files: string array)
    (clientPath: string)
    (info: ShareInfo)
    (fileExists: string -> bool)
    (fileDelete: string -> unit)
    (readAllText: string -> string)
    (zipOpen: string -> ZipArchiveMode -> IZipArchive)
    =
    try
      if fileExists targetZip then
        fileDelete targetZip

      use zip = zipOpen targetZip ZipArchiveMode.Create

      files
      |> Array.iter (fun file ->
        let projPath = Path.GetDirectoryName clientPath

        let relativePath =
          file.Replace(projPath, "")
          |> fun c ->
            if c.StartsWith Path.DirectorySeparatorChar then
              c.TrimStart(Path.DirectorySeparatorChar)
            else
              c

        let entry = zip.CreateEntry relativePath
        use writer = new StreamWriter(entry.Open())
        writer.Write(readAllText file))

      let entry = zip.CreateEntry(ShareInfo.FileName)
      use writer = new StreamWriter(entry.Open())
      writer.Write(Instance.jsonSerialize info)

      Res.ok targetZip
    with exc ->
      Res.failed exc
  //let private createZip (io: IOs) targetZip (files: string array) (clientPath: string) (info: ShareInfo) =
  //  ()

  let private getClientFiles (client: ClientGroup) (fileExists: string -> bool) (getFiles: string * string * SearchOption -> string array) =
    let clientPath = client.Path
    let clientConfig = Path.Combine(clientPath, ClientGroup.ConfigFilename)

    if not (fileExists clientConfig) then
      Res.failedWith $"{clientPath} is not a valid client path"
    else
      let filesToZip =
        getFiles (clientPath, "*.*", SearchOption.AllDirectories)
        |> Array.filter (fun f -> not (Path.GetFileName(f) = ProjectSaveState.FileName))
        |> Array.filter (fun f ->
          let parentDir = f |> Path.GetDirectoryName |> Path.GetFileName
          not (parentDir = Project.AutoSaveFolderName))

      Res.ok filesToZip

  let createInfo clientName shareType =
    { Version = Utils.appVersionSimple
      Source = RuntimeInformation.RuntimeIdentifier
      CreatedAt = DateTime.Now
      ClientName = clientName
      Type = shareType }


  let _createFileShare
    (targetZip: string)
    (files: string array)
    (client: ClientGroup)
    (fileExists: string -> bool)
    (getFiles: string * string * SearchOption -> string array)
    (fileDelete: string -> unit)
    (readAllText: string -> string)
    (zipOpen: string -> ZipArchiveMode -> IZipArchive)
    =

    let methodsPath = Project.getMethodsPath client.Path

    let filterOutMethodFiles (files: string array) =
      seq {
        //take the files (like, *.cs, config.json) that are not under a method folder
        //need those to recreate the client when the zip is imported
        for file in files do
          if not (file.StartsWith methodsPath) then
            yield file
      }

    (getClientFiles client fileExists getFiles)
    |> Res.map (fun allFiles -> (filterOutMethodFiles allFiles) |> Seq.append files |> Seq.toArray)
    |> Res.map (fun targetFiles ->
      let info = createInfo client.Name ShareInfo.FileShare
      createZip targetZip targetFiles client.Path info fileExists fileDelete readAllText zipOpen)
    |> Res.getValue

  let createFileShare (io: IOs) (targetZip: string) (files: string array) (client: ClientGroup) =
    _createFileShare targetZip files client io.File.Exists io.Dir.GetFiles io.File.Delete io.File.ReadAllText io.Zip.Open

  let _createFolderShare
    (targetZip: string)
    (methodName: string)
    (client: ClientGroup)
    (fileExists: string -> bool)
    (getFiles: string * string * SearchOption -> string array)
    (fileDelete: string -> unit)
    (readAllText: string -> string)
    (zipOpen: string -> ZipArchiveMode -> IZipArchive)
    =
    let methodPath = Project.getMethodPath client.Path methodName
    let methodsPath = Project.getMethodsPath client.Path

    let filterForMethodFiles (files: string array) =
      seq {
        for file in files do
          if (file.StartsWith methodsPath) then
            if (file.StartsWith methodPath) then
              yield file
          else
            yield file
      }

    getClientFiles client fileExists getFiles
    |> Res.map (fun files -> (filterForMethodFiles files) |> Seq.toArray)
    |> Res.map (fun files ->
      let info = createInfo client.Name ShareInfo.FileShare
      createZip targetZip files client.Path info fileExists fileDelete readAllText zipOpen)
    |> Res.getValue

  let createFolderShare (io: IOs) (targetZip: string) (methodName: string) (client: ClientGroup) =
    _createFolderShare targetZip methodName client io.File.Exists io.Dir.GetFiles io.File.Delete io.File.ReadAllText io.Zip.Open

  let _createClientShare
    (targetZip: string)
    (client: ClientGroup)
    (fileExists: string -> bool)
    (getFiles: string * string * SearchOption -> string array)
    (fileDelete: string -> unit)
    (readAllText: string -> string)
    (zipOpen: string -> ZipArchiveMode -> IZipArchive)
    =
    let clientPath = client.Path
    let clientConfig = Path.Combine(clientPath, ClientGroup.ConfigFilename)

    if not (fileExists clientConfig) then
      Res.failedWith $"{clientPath} is not a valid client path"
    else
      let filesToZip =
        getFiles (clientPath, "*.*", SearchOption.AllDirectories)
        |> Array.filter (fun f -> not (Path.GetFileName(f) = ProjectSaveState.FileName))
        |> Array.filter (fun f ->
          let parentDir = f |> Path.GetDirectoryName |> Path.GetFileName
          not (parentDir = Project.AutoSaveFolderName))

      let info = createInfo client.Name ShareInfo.ClientShare
      createZip targetZip filesToZip clientPath info fileExists fileDelete readAllText zipOpen

  let createClientShare (io: IOs) (targetZip: string) (client: ClientGroup) =
    _createClientShare targetZip client io.File.Exists io.Dir.GetFiles io.File.Delete io.File.ReadAllText io.Zip.Open


  let importInto (io: IOs) (project: Project) (zip: string) =
    let allowMultiple ext =
      let extensions = [| $"{Ext.requestFileExt}" |]
      extensions |> Array.tryFind (fun c -> c = ext) |> Option.isSome

    use zipArchive = io.Zip.OpenRead zip

    let info =
      zipArchive.Entries |> Seq.find (fun entry -> entry.Name = ShareInfo.FileName)

    use reader = new StreamReader(info.Open())
    let i = reader.ReadToEnd() |> Instance.jsonDeserialize<ShareInfo>
    let clientName = i.ClientName
    let clientOpt = project.Clients |> Array.tryFind (fun c -> c.Name = clientName)
    let mutable updated = false

    match clientOpt with
    | Some(client) ->
     
      for entry: ZipArchiveEntry in zipArchive.Entries do
        let target = Path.Combine(project.Path, entry.FullName) |> Path.GetFullPath
        let dir = Path.GetDirectoryName target
        io.Dir.CreateDirectory dir
        let ext = Path.GetExtension target

        if (io.File.Exists target) then
          if (allowMultiple ext) then
            let fileStart = Path.GetFileNameWithoutExtension target

            let newTarget =
              Utils.getAvailableFileName dir fileStart ext |> fun n -> Path.Combine(dir, n)

            updated <- true
            entry.ExtractToFile(newTarget)
        else
          updated <- true
          entry.ExtractToFile(target)
    | None ->
      //extract away
      io.Zip.ExtractToDirectory zip project.Path false
      updated <- true

    let infoFile = Path.Combine(project.Path, ShareInfo.FileName)
    if (File.Exists infoFile) then
      io.File.Delete infoFile
   
    //return
    clientName, updated
