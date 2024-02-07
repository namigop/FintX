module ShareTests

open System
open Tefin.Core
open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open Xunit
open IoMock
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

let testClientConfigContent =
  @$"{{
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

let projectSaveStateFile =
  { Path = ProjectSaveState.FileName
    Content = testProjSaveStateContent }

let methodFolder1 =
  { Path = testMethodName1
    Files =
      [| { Path = testFile1Request
           Content = testFile1Content } |]
    Folders =
      [| { Path = Project.AutoSaveFolderName
           Files = Array.empty
           Folders = Array.empty } |] }

let methodFolder2 =
  { Path = testMethodName2
    Files =
      [| { Path = testFile2Request
           Content = testFile2Content } |]
    Folders =
      [| { Path = Project.AutoSaveFolderName
           Files = Array.empty
           Folders = Array.empty } |] }

let codeFolder =
  { Path = "code"
    Files =
      [| { Path = "Client.cs"
           Content = "not used" }
         { Path = "Client.g.cs"
           Content = "not used" } |]
    Folders = Array.empty }

let buildProjectFolder projectName projFiles clientName clientConfigContent methodFolders =
  { Path = projectName
    Files = projFiles
    Folders =
      [| { Path = clientName
           Files =
             [| { Path = ClientGroup.ConfigFilename
                  Content = clientConfigContent } |]
           Folders =
             [| codeFolder
                { Path = "methods"
                  Files = Array.empty
                  Folders = methodFolders } |] } |] }

let projectFolder =
  buildProjectFolder testProjectName [| projectSaveStateFile |] testClientName testClientConfigContent [| methodFolder1; methodFolder2 |]

let buildProjectsFolder projFolder =
  { Path = "projects"
    Files = Array.empty
    Folders = [| projFolder |] }

let projectsFolder =
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
  buildProjectsFolder projectFolder

  

[<Fact>]
let ``Can create zip for client`` () =
  let sep = Path.DirectorySeparatorChar
  let projPath = $"projects{sep}{testProjectName}"
  let reqFile = $"projects{sep}{testProjectName}{sep}{testClientName}{sep}methods{sep}{testMethodName1}{sep}{testFile1Request}"
  let clientPath = $"projects{sep}{testProjectName}{sep}{testClientName}"
  let zipFile = $"{sep}abc{sep}def{sep}hij.zip"
  let io = ioMock projectsFolder

  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists

  let filesToZip = [|reqFile|]
  let client = proj.Clients[0]
  
  let fileDelete file =
    ()
  let fileExists file =
    true
    
  let mutable archive = Unchecked.defaultof<IZipArchive2>
  let zipOpen file mode =
    let z = zipMock.Open file mode
    archive <- z :?> IZipArchive2
    z
  
  let allFiles =
    io.GetFiles($"projects{sep}{testProjectName}{sep}{testClientName}", "*.*", SearchOption.AllDirectories)
    |> Array.sortDescending
  
  let zip = Share._createFileShare zipFile filesToZip client fileExists io.GetFiles fileDelete io.ReadAllText zipOpen
  Assert.True( Res.isOk zip)
  Assert.Equal(zipFile, Res.getValue zip)
  
  let entries = archive.GetEntries()
  let zipEntries = entries |> Array.sortByDescending (fun e -> e.Path)
  
  let isPartOfZip (file:string) =
    entries
    |> Array.tryFind (fun e ->
      Path.GetFileName e.Path = Path.GetFileName file)
    |> Option.isSome
    
  let csFiles = io.GetFiles(projPath, "*.cs", SearchOption.AllDirectories)
  let clientConfig = io.GetFiles(projPath, "config.json", SearchOption.AllDirectories)
  let expected = csFiles |> Array.append filesToZip |> Array.append clientConfig
  for e in expected do
    Assert.True(isPartOfZip e)
    
  Assert.Equal(allFiles.Length, zipEntries.Length)
   
     
