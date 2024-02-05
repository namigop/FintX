module ProjectTests

open System
open Tefin.Core
open Tefin.Core.Interop
open Xunit
open System.IO

let testProjectName = "myProject"
let testClientName = "myClient"
let testMethodName1 = "myMethod1"
let testMethodName2 = "myMethod2"
let testFile1Request = "file1.fxrq"
let testFile1Content = "{}"

let testProjSaveStateContent =
  @$"{{
    ""Package"": ""grpc"",
    ""ClientState"": [
        {{
            ""Name"": ""{testClientName}"",
            ""OpenFiles"": [
                ""projects/{testProjectName}/{testClientName}/methods/{testMethodName1}/file1.fxrq""
            ]
        }}
    ]
}}"

let testClientConfigContent = @$"{{
    ""Name"": ""{testClientName}"",
    ""ServiceName"": ""greet.Greeter"",
    ""Url"": ""http://localhost:5070"",
    ""IsUsingSSL"": false,
    ""Jwt"": """",
    ""Description"": """",
    ""IsCertFromFile"": false,
    ""CertStoreLocation"": ""LocalMachine"",
    ""CertThumbprint"": """"
}}"

let testFile2Request = "file2.fxrq"
let testFile2Content = "{}"

type File = { Path: string; Content: string }

type Folder =
  { Path: string
    Files: File array
    Folders: Folder array }

let projectFolder =
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

  { Path = "projects"
    Files = Array.empty
    Folders =
      [| { Path = testProjectName
           Files =
             [| { Path = ProjectSaveState.FileName
                  Content = testProjSaveStateContent } |]
           Folders =
             [| { Path = testClientName
                  Files =
                    [| { Path = ClientGroup.ConfigFilename
                         Content = testClientConfigContent } |]
                  Folders =
                    [| { Path = "code"
                         Files =
                           [| { Path = "Client.cs"
                                Content = "not used" }
                              { Path = "Client.g.cs"
                                Content = "not used" } |]
                         Folders = Array.empty }
                       { Path = "methods"
                         Files = Array.empty
                         Folders =
                           [| { Path = testMethodName1
                                Files =
                                  [| { Path = testFile1Request
                                       Content = testFile1Content } |]
                                Folders =
                                  [| { Path = Project.AutoSaveFolderName
                                       Files = Array.empty
                                       Folders = Array.empty } |] }
                              { Path = testMethodName2
                                Files =
                                  [| { Path = testFile2Request
                                       Content = testFile2Content } |]
                                Folders =
                                  [| { Path = Project.AutoSaveFolderName
                                       Files = Array.empty
                                       Folders = Array.empty } |] } |] }

                       |] } |] } |] }

type IoMock =
  { GetFiles: string * string * SearchOption -> string array
    ReadAllText: string -> string
    CreateDirectory: string -> unit
    GetDirectories: string -> string array }

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
          let fullPath = $"{p}/{f.Path}"
          { f with Path = fullPath })
      else
        folder.Folders
        |> Array.collect (fun f -> getFilesRec f pattern option pathParts "")
        |> Array.filter (fun f -> matchFilePattern f pattern)
    else
      folder.Files
      |> Array.filter (fun f -> matchFilePattern f pattern)
      |> Array.map (fun f ->
        let fullPath = $"{p}/{f.Path}"
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

  { GetFiles = fun c -> (getFiles c) |> Array.map (fun f -> f.Path)
    ReadAllText = readAllText
    CreateDirectory = createDirectory
    GetDirectories = fun d -> (getDirectories d) |> Array.map (fun f -> f.Path) }

[<Fact>]
let ``Can load project`` () =
  let projPath = $"projects/{testProjectName}"
  let clientConfig = $"projects/{testProjectName}/{testClientName}/{ClientGroup.ConfigFilename}"
  let clientPath = $"projects/{testProjectName}/{testClientName}"
  
  let io = ioMock projectFolder
  let proj = Project._loadProject
               projPath
               io.GetFiles
               io.ReadAllText
               io.CreateDirectory
               io.GetDirectories
               
  Assert.Equal(testProjectName, proj.Name)
  Assert.Equal(1, proj.Clients.Length)
  
  let client = proj.Clients[0]
  Assert.Equal(testClientName, client.Name)
  Assert.Equal(2, client.Methods.Length)
  Assert.True(Option.isSome client.Config)
  Assert.Equal(2, client.CodeFiles.Length)
  Assert.Equal(clientConfig, client.ConfigFile.Replace("\\", "/"))
  Assert.Equal(clientPath, client.Path.Replace("\\", "/"))
  
  let method1 = client.Methods[0]
  let method2 = client.Methods[1]

  Assert.Equal(testMethodName1, method1.Name)
  Assert.Equal(testMethodName2, method2.Name)
  Assert.Equal(1, method1.RequestFiles.Length)
  let req1 =  method1.RequestFiles[0].Replace("\\", "/")
  let req1File = $"projects/{testProjectName}/{testClientName}/methods/{testMethodName1}/{testFile1Request}"
  Assert.Equal(req1File, req1)
  
  Assert.Equal(1, method2.RequestFiles.Length)
  let req2 =  method2.RequestFiles[0].Replace("\\", "/")
  let req2File = $"projects/{testProjectName}/{testClientName}/methods/{testMethodName2}/{testFile2Request}"
  Assert.Equal(req2File, req2)
  
  