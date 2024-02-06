module ProjectTests

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
let ``Can add client`` () =
  task {
    let projPath = $"projects/{testProjectName}"

    let clientConfig =
      $"projects/{testProjectName}/{testClientName}/{ClientGroup.ConfigFilename}"

    let clientPath = $"projects/{testProjectName}/{testClientName}"

    let newClient = "fooBarTestClient"
    let newCsFile = "/abc/def/Greeter.cs"
    let io = ioMock projectsFolder

    let proj =
      Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists

    let createDir (dir:string) =
      //newly created folders will contain the new client name      
      Assert.True (dir.Contains(newClient))
     
    let fileCopy (source: string, target: string, overwrite: bool) =
      //this will be called to copy the cs files
      Assert.Equal(newCsFile, source)
      
      let target = target.Replace("\\", "/")
      let expected = $"projects/{testProjectName}/{newClient}/code/{Path.GetFileName(source)}"
      Assert.Equal(expected, target)     

    let moveDir (fromPath: string) (toPath: string) =
      let old = Path.GetFileName fromPath
      let target = Path.GetFileName toPath
      ()
    // Assert.Equal(old, testClientName)
    // Assert.Equal(clientName, target)

    let writeAllTextAsync (file: string) (content: string) =
      task {
        let expected =
          $"projects/{testProjectName}/{newClient}/{ClientGroup.ConfigFilename}"

        Assert.Equal(expected, file.Replace("\\", "/"))
        //Assert.Equal(updatedConfigContent, content)

        do! System.Threading.Tasks.Task.Yield()
      }
      |> fun t -> t :> System.Threading.Tasks.Task


    let updateFolder =
      buildProjectFolder
        testProjectName
        [| projectSaveStateFile |]
        newClient
        testClientConfigContent
        [| methodFolder1; methodFolder2 |]
      |> fun proj -> buildProjectsFolder proj

    let io2 = ioMock updateFolder

    do!
      Project._addClient
        proj
        newClient
        "serviceName"
        "protoUrl.proto"
        "my desc"
        [| newCsFile |]
        createDir
        fileCopy
        moveDir
        writeAllTextAsync
        io2.ReadAllText
        io2.GetDirectories
        io2.GetFiles
    }

[<Fact>]
let ``Can update client config`` () =
  task {
    let clientPath = $"projects/{testProjectName}/{testClientName}"
    let clientConfig = $"{clientPath}/{ClientGroup.ConfigFilename}"
    let description = $"my new desc"
    let serviceName = $"svc"
    let protoOrUrl = $"/foo/bar/mynew.proto"
    let clientName = $"mynewclientName"

    let updatedConfig =
      ClientConfig(Description = description, ServiceName = serviceName, Url = protoOrUrl, Name = clientName)

    let updatedConfigContent = Instance.jsonSerialize updatedConfig

    let moveDir (fromPath: string) (toPath: string) =
      let old = Path.GetFileName fromPath
      let target = Path.GetFileName toPath
      Assert.Equal(old, testClientName)
      Assert.Equal(clientName, target)

    let writeAllTextAsync (file: string) (content: string) =
      task {
        let expected =
          $"projects/{testProjectName}/{clientName}/{ClientGroup.ConfigFilename}"

        Assert.Equal(expected, file.Replace("\\", "/"))
        Assert.Equal(updatedConfigContent, content)

        do! System.Threading.Tasks.Task.Yield()
      }
      |> fun t -> t :> System.Threading.Tasks.Task

    let updateFolder =
      buildProjectFolder testProjectName [| projectSaveStateFile |] clientName updatedConfigContent [| methodFolder1; methodFolder2 |]
      |> buildProjectsFolder

    let io = ioMock updateFolder

    GlobalHub.subscribe(Action<MsgClientUpdated>(fun c ->
      Assert.Equal(clientName, c.Client.Name)
      Assert.Equal(clientName, c.Client.Config.Value.Name)
      Assert.Equal(protoOrUrl, c.Client.Config.Value.Url)
      Assert.Equal(serviceName, c.Client.Config.Value.ServiceName)
      Assert.Equal(description, c.Client.Config.Value.Description)
       ))
    |> ignore
    
    do!
      Project._updateClientConfig
        clientConfig
        updatedConfig
        moveDir
        writeAllTextAsync
        io.ReadAllText
        io.CreateDirectory
        io.GetDirectories
        io.GetFiles

  }

[<Fact>]
let ``Can load project with no save state`` () =
  let stillValidProj =
    buildProjectFolder "proj1" Array.empty testClientName testClientConfigContent [| methodFolder1; methodFolder2 |]

  let io = ioMock stillValidProj
  let projPath = $"projects/proj1"

  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists

  //if we have an instance of proj, then it loaded just fine
  Assert.Equal("proj1", proj.Name)

  let stillValidProj2 =
    buildProjectFolder
      "proj2"
      [| { Path = ProjectSaveState.FileName
           Content = "" } |]
      testClientName
      testClientConfigContent
      [| methodFolder1; methodFolder2 |]

  let io2 = ioMock stillValidProj2
  let projPath2 = $"projects/proj2"

  let proj2 =
    Project._loadProject projPath2 io2.GetFiles io2.ReadAllText io2.CreateDirectory io2.GetDirectories io2.FileExists

  //if we have an instance of proj, then it loaded just fine
  Assert.Equal("proj2", proj2.Name)


[<Fact>]
let ``Can load valid project`` () =
  let projPath = $"projects/{testProjectName}"

  let clientConfig =
    $"projects/{testProjectName}/{testClientName}/{ClientGroup.ConfigFilename}"

  let clientPath = $"projects/{testProjectName}/{testClientName}"

  let io = ioMock projectsFolder

  let proj =
    Project._loadProject projPath io.GetFiles io.ReadAllText io.CreateDirectory io.GetDirectories io.FileExists

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
  let req1 = method1.RequestFiles[0].Replace("\\", "/")

  let req1File =
    $"projects/{testProjectName}/{testClientName}/methods/{testMethodName1}/{testFile1Request}"

  Assert.Equal(req1File, req1)

  Assert.Equal(1, method2.RequestFiles.Length)
  let req2 = method2.RequestFiles[0].Replace("\\", "/")

  let req2File =
    $"projects/{testProjectName}/{testClientName}/methods/{testMethodName2}/{testFile2Request}"

  Assert.Equal(req2File, req2)
