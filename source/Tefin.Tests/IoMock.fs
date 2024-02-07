module IoMock

open System
open System.IO
open System.IO.Compression
open Tefin.Core


type File = { Path: string; Content: string }

type Folder =
  { Path: string
    Files: File array
    Folders: Folder array }


type IoMock =
  { GetFiles: string * string * SearchOption -> string array
    ReadAllText: string -> string
    CreateDirectory: string -> unit
    GetDirectories: string -> string array
    FileExists: string -> bool }

type IZipEntry2 =
  inherit IZipEntry
  abstract Path : string
type IZipArchive2 =
  inherit IZipArchive
  abstract GetEntries : unit -> IZipEntry2 array
  
let sep = Path.DirectorySeparatorChar
let zipMock =
  let createEntry (path:string) =
    {new IZipEntry2 with
      member x.Open () = new MemoryStream()
      member x.Path = path
    }
    
  let createArchive (file:string) (mode:ZipArchiveMode) =
    let items = ResizeArray<IZipEntry2>()
    { new IZipArchive2 with
        member x.CreateEntry (path) =
          let item = createEntry path
          items.Add item
          item
        member x.GetEntries() = items.ToArray()
        member x.Dispose() = ()          
    }
    
  let zipIO =
    { new IZipIO with
        member x.Open z m = createArchive z m
        member x.OpenRead file = Unchecked.defaultof<ZipArchive>
        member x.ExtractToDirectory zipFile targetDir overwrite = () }
  zipIO
let ioMock (rootFolder:Folder) =
  let matchFilePattern (file: File) (pattern: string) =
    if (pattern = "*.*") then
      true     
    elif (pattern.StartsWith("*.")) then
      let ext = pattern.Substring(1)
      file.Path.EndsWith(ext)
    else
      file.Path = pattern

  let rec getDirRec (folder: Folder) (targetPath: string) (currentPath: string) =
    if currentPath = targetPath then
      folder.Folders
      |> Array.map (fun folder -> {folder with Path = $"{currentPath}{sep}{folder.Path}" })
    else
      folder.Folders
      |> Array.collect (fun f -> getDirRec f targetPath $"{currentPath}{sep}{f.Path}")

  let rec getFilesRec (folder: Folder) pattern (option: SearchOption) (pathParts: string array) (p: string) =
    let targetPath = String.Join($"{sep}", pathParts)
    if option = SearchOption.TopDirectoryOnly then
      if (pathParts[pathParts.Length - 1] = folder.Path) then
        folder.Files
        |> Array.filter (fun f -> matchFilePattern f pattern)
        |> Array.map (fun f ->
          let p = String.Join($"{sep}", pathParts)
          let fullPath = $"{p}{sep}{f.Path}" 
          { f with Path = fullPath })
        |> fun files -> folder, files
      else
        folder.Folders
        |> Array.collect (fun f ->
            let folder,files = getFilesRec f pattern option pathParts ""
            files)
        |> Array.filter (fun f -> matchFilePattern f pattern)
        |> fun files -> folder, files
    else
      folder.Files
      |> Array.filter (fun f -> matchFilePattern f pattern)
      |> Array.map (fun f ->
        let fullPath = $"{p}{sep}{f.Path}" 
        { f with Path = fullPath })
      |> Array.filter (fun f ->
          f.Path.StartsWith targetPath)
      |> Array.append (
        folder.Folders
        |> Array.collect (fun f ->
          let folder, files = getFilesRec f pattern option pathParts ($"{p}{sep}{f.Path}")
          files)
      )
      |> fun files -> folder, files
      
  let getFolder (path: string) (pattern: string) (options: SearchOption) =
     
    let pathParts = path.Split($"{sep}")
    getFilesRec rootFolder pattern options pathParts rootFolder.Path
    
    
  let getFiles (path: string, pattern: string, options: SearchOption) =    
    let _, files = getFolder path pattern options
    files

  let readAllText (file: string) =
    
    let dir = Path.GetDirectoryName file

    let boo = getFiles (dir, "*.*", SearchOption.AllDirectories)
    
    boo
    |> Array.tryFind (fun f -> f.Path = file)
    |> fun m ->
        match m with
        | Some(f) -> f.Content
        | None -> raise (FileNotFoundException("file not found", file))

  let createDirectory name = ()

  let getDirectories (targetPath: string) =
    
    getDirRec rootFolder targetPath rootFolder.Path
    
  let fileExists (file:string) =
     
    let dir = Path.GetDirectoryName file

    getFiles (dir, "*.*", SearchOption.AllDirectories)
    |> Array.tryFind (fun f -> f.Path = file)
    |> Option.isSome
    

  { GetFiles = fun c -> (getFiles c) |> Array.map (fun f -> f.Path)
    ReadAllText = readAllText
    CreateDirectory = createDirectory
    GetDirectories = fun d -> (getDirectories d) |> Array.map (fun f -> f.Path)
    FileExists = fileExists }


