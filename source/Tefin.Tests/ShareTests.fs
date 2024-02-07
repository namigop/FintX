module ShareTests

open System
open Tefin.Core
open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open Xunit
open IoMock
open System.IO
let sep = Path.DirectorySeparatorChar
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
  |> fun c ->
    if Utils.isWindows() then
      c.Replace("/", "\\\\")
    else
      c
  

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
let ``Can create zip for method`` () = 
  let projPath = $"projects{sep}{testProjectName}"
  let zipFile = $"{sep}abc{sep}def{sep}hij.zip"
  let io = ioMock projectsFolder
  let methodName = testMethodName2
  let methodPath = $"projects{sep}{testProjectName}{sep}{testClientName}{sep}methods{sep}{methodName}"
  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists


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
  
  let zip = Share._createFolderShare zipFile methodName client fileExists io.GetFiles fileDelete io.ReadAllText zipOpen
  Assert.True( Res.isOk zip)
  Assert.Equal(zipFile, Res.getValue zip)
 
  let entries = archive.GetEntries() 
  let isPartOfZip (file:string) =
    entries
    |> Array.tryFind (fun e ->
      if not (e.Path = Share.ShareInfo.FileName) then
        Assert.True(e.Path.StartsWith($"{testClientName}")) //zipping starts from client folder
      
      Path.GetFileName e.Path = Path.GetFileName file)
    |> Option.isSome
     
  let filesToZip = io.GetFiles(methodPath, "*.*", SearchOption.AllDirectories)
  let csFiles = io.GetFiles(projPath, "*.cs", SearchOption.AllDirectories)
  let clientConfig = io.GetFiles(projPath, "config.json", SearchOption.AllDirectories)
  let expected = csFiles |> Array.append filesToZip |> Array.append clientConfig
  for e in expected do
    Assert.True(isPartOfZip e, $"File {e} is missing from the zip")
    
  //Should contain share.info at the root
  Assert.True(isPartOfZip Share.ShareInfo.FileName)

  

[<Fact>]
let ``Can create zip for client`` () = 
  let projPath = $"projects{sep}{testProjectName}"
  let zipFile = $"{sep}abc{sep}def{sep}hij.zip"
  let io = ioMock projectsFolder

  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists


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
  
  let zip = Share._createClientShare zipFile client fileExists io.GetFiles fileDelete io.ReadAllText zipOpen
  Assert.True( Res.isOk zip)
  Assert.Equal(zipFile, Res.getValue zip)
 
  let entries = archive.GetEntries() 
  let isPartOfZip (file:string) =
    entries
    |> Array.tryFind (fun e ->
      if not (e.Path = Share.ShareInfo.FileName) then
        Assert.True(e.Path.StartsWith($"{testClientName}")) //zipping starts from client folder
      
      Path.GetFileName e.Path = Path.GetFileName file)
    |> Option.isSome
     
  let expected =
    io.GetFiles(projPath, "*.*", SearchOption.AllDirectories)
    //note: Project save state is not part of the zip file
    |> Array.filter (fun p -> not <| p.EndsWith($"{ProjectSaveState.FileName}"))
    
  for e in expected do
    Assert.True(isPartOfZip e, $"File missing from zip: {e}")
   
  //Should contain share.info at the root
  Assert.True(isPartOfZip Share.ShareInfo.FileName, $"{Share.ShareInfo.FileName} missing from zip")

[<Fact>]
let ``Can create zip for selected files`` () =
 
  let projPath = $"projects{sep}{testProjectName}"
  let reqFile1 = $"projects{sep}{testProjectName}{sep}{testClientName}{sep}methods{sep}{testMethodName1}{sep}{testFile1Request}"
  let reqFile2 = $"projects{sep}{testProjectName}{sep}{testClientName}{sep}methods{sep}{testMethodName2}{sep}{testFile2Request}"
  let clientPath = $"projects{sep}{testProjectName}{sep}{testClientName}"
  let zipFile = $"{sep}abc{sep}def{sep}hij.zip"
  let io = ioMock projectsFolder

  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists

  let filesToZip = [|reqFile1; reqFile2|]
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
  
  let zip = Share._createFileShare zipFile filesToZip client fileExists io.GetFiles fileDelete io.ReadAllText zipOpen
  Assert.True( Res.isOk zip)
  Assert.Equal(zipFile, Res.getValue zip)
  
  let entries = archive.GetEntries()
 
  let isPartOfZip (file:string) =
    entries
    |> Array.tryFind (fun e ->
      if not (e.Path = Share.ShareInfo.FileName) then
        Assert.True(e.Path.StartsWith($"{testClientName}")) //zipping starts from client folder
        
      Path.GetFileName e.Path = Path.GetFileName file)
    |> Option.isSome
    
  let csFiles = io.GetFiles(projPath, "*.cs", SearchOption.AllDirectories)
  let clientConfig = io.GetFiles(projPath, "config.json", SearchOption.AllDirectories)
  let expected = csFiles |> Array.append filesToZip |> Array.append clientConfig
  for e in expected do
    Assert.True(isPartOfZip e, $"File {e} is missing from the zip")
    
  //Should contain share.info at the root
  Assert.True(isPartOfZip Share.ShareInfo.FileName)
    
 
   
     
