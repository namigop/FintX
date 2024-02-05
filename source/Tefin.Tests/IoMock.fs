module IoMock

open System
open System.IO


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

let ioMock (rootFolder:Folder) =
  let matchFilePattern (file: File) (pattern: string) =
    if (pattern = "*.*") then
      true     
    elif (pattern.StartsWith("*.")) then
      let ext = Path.GetExtension file.Path
      file.Path.EndsWith(ext)
    else
      file.Path = pattern

  let rec getDirRec (folder: Folder) (targetPath: string) (currentPath: string) =
    if currentPath = targetPath then
      folder.Folders
      |> Array.map (fun folder -> {folder with Path = $"{currentPath}/{folder.Path}" })
    else
      folder.Folders
      |> Array.collect (fun f -> getDirRec f targetPath $"{currentPath}/{f.Path}")

  let rec getFilesRec (folder: Folder) pattern (option: SearchOption) (pathParts: string array) (p: string) =
    let targetPath = String.Join("/", pathParts)
    if option = SearchOption.TopDirectoryOnly then
      if (pathParts[pathParts.Length - 1] = folder.Path) then
        folder.Files
        |> Array.filter (fun f -> matchFilePattern f pattern)
        |> Array.map (fun f ->
          let p = String.Join("/", pathParts)
          let fullPath = $"{p}/{f.Path}".Replace("\\", "/")
          { f with Path = fullPath })
      else
        folder.Folders
        |> Array.collect (fun f -> getFilesRec f pattern option pathParts "")
        |> Array.filter (fun f -> matchFilePattern f pattern)
    else
      folder.Files
      |> Array.filter (fun f -> matchFilePattern f pattern)
      |> Array.map (fun f ->
        let fullPath = $"{p}/{f.Path}".Replace("\\", "/")
        { f with Path = fullPath })
      |> Array.filter (fun f ->
          f.Path.StartsWith targetPath)
      |> Array.append (
        folder.Folders
        |> Array.collect (fun f -> getFilesRec f pattern option pathParts ($"{p}/{f.Path}"))
      )

  let getFiles (path: string, pattern: string, options: SearchOption) =
    let path = path.Replace("\\", "/")
    let pathParts = path.Split("/")
    getFilesRec rootFolder pattern options pathParts rootFolder.Path

  let readAllText (file: string) =
    let file = file.Replace("\\", "/")
    let dir = Path.GetDirectoryName file

    getFiles (dir, "*.*", SearchOption.AllDirectories)
    |> Array.tryFind (fun f -> f.Path = file)
    |> fun m ->
        match m with
        | Some(f) -> f.Content
        | None -> raise (FileNotFoundException("file not found", file))

  let createDirectory name = ()

  let getDirectories (targetPath: string) =
    let targetPath = targetPath.Replace("\\", "/")
    getDirRec rootFolder targetPath rootFolder.Path
    
  let fileExists (file:string) =
    let file = file.Replace("\\", "/")
    let dir = Path.GetDirectoryName file

    getFiles (dir, "*.*", SearchOption.AllDirectories)
    |> Array.tryFind (fun f -> f.Path = file)
    |> Option.isSome
    

  { GetFiles = fun c -> (getFiles c) |> Array.map (fun f -> f.Path)
    ReadAllText = readAllText
    CreateDirectory = createDirectory
    GetDirectories = fun d -> (getDirectories d) |> Array.map (fun f -> f.Path)
    FileExists = fileExists }


