namespace Tefin.Core

open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open System.IO

type AddClientRequest =
  { Name: string
    ReflectionServiceUrl: string
    //CsFiles
    ProtoFiles: string array
    Desc: string }

module Project =

  let AutoSaveFolderName = "_autoSave"
  let getMethodsPath (clientPath: string) = Path.Combine(clientPath, "methods")

  let getMethodPath (clientPath: string) (methodName: string) =
    getMethodsPath clientPath |> fun m -> Path.Combine(m, methodName)

  let getAutoSavePath (clientPath: string) (methodName: string) =
    getMethodPath clientPath methodName
    |> fun m -> Path.Combine(m, AutoSaveFolderName)

  let _loadMethods
    (clientPath: string)
    (createDirectory: string -> unit)
    (getDirectories: string -> string array)
    (getFiles: string * string * SearchOption -> string array)
    =
    let methodPath = getMethodsPath clientPath
    createDirectory methodPath
    let methodDirs = getDirectories methodPath //"*.*" SearchOption.TopDirectoryOnly)

    methodDirs
    |> Array.map (fun m ->
      let methodName = Path.GetFileName m

      let requestFiles =
        getFiles (m, "*" + Ext.requestFileExt, SearchOption.AllDirectories)
        |> Array.filter (fun fp -> not <| fp.Contains(AutoSaveFolderName)) //ignore auto-saved files

      { RequestFiles = requestFiles
        Name = methodName
        Path = m })

  let loadMethods (io: IOResolver) (clientPath: string) =
    _loadMethods clientPath io.Dir.CreateDirectory io.Dir.GetDirectories io.Dir.GetFiles

  let _loadClient
    (clientPath: string)
    (readAllText: string -> string)
    (createDirectory: string -> unit)
    (getDirectories: string -> string array)
    (getFiles: string * string * SearchOption -> string array)
    =
    let configFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)
    let config = Instance.jsonDeserialize<ClientConfig> (readAllText configFile)

    let codePath = Path.Combine(clientPath, "code")
    let files = Directory.GetFiles(codePath, "*.*")

    { ConfigFile = configFile
      Config = Some config
      CodeFiles = files
      Methods = _loadMethods clientPath createDirectory getDirectories getFiles

      Name = config.Name
      Path = clientPath }

  let loadClient (io: IOResolver) (clientPath: string) =
    _loadClient clientPath io.File.ReadAllText io.Dir.CreateDirectory io.Dir.GetDirectories io.Dir.GetFiles

  let _loadProject
    (projectPath: string)
    (getFiles: string * string * SearchOption -> string array)
    (readAllText: string -> string)
    (createDirectory: string -> unit)
    (getDirectories: string -> string array)
    =
    let clientPaths =
      getFiles (projectPath, ClientGroup.ConfigFilename, SearchOption.AllDirectories)
      |> Array.map (fun file -> Path.GetDirectoryName file)

    let projSaveState =
      let content = Path.Combine(projectPath, ProjectSaveState.FileName) |> readAllText

      if (System.String.IsNullOrEmpty content) then
        ProjectSaveState.Empty("grpc")
      else
        Instance.jsonDeserialize<ProjectSaveState> (content)

    let projectName = Path.GetFileName projectPath

    let clients =
      clientPaths
      |> Array.map (fun path -> _loadClient path readAllText createDirectory getDirectories getFiles)

    let config = Path.Combine(projectPath, Project.ProjectConfigFileName)

    { Name = projectName
      Package = projSaveState.Package
      Clients = clients
      ConfigFile = config
      Path = projectPath }

  let loadProject (io: IOResolver) (projectPath: string) =
    _loadProject projectPath io.Dir.GetFiles io.File.ReadAllText io.Dir.CreateDirectory io.Dir.GetDirectories

  let createSaveState (io: IOResolver) package (projectPath: string) =
    let state =
      { Package = package
        ClientState = Array.empty }

    let file = Path.Combine(projectPath, ProjectSaveState.FileName)
    let content = Instance.jsonSerialize state
    io.File.WriteAllText file content

  let getSaveState (io: IOResolver) (projectPath: string) =
    let file = Path.Combine(projectPath, ProjectSaveState.FileName)

    let saveState =
      let content = io.File.ReadAllText file

      if System.String.IsNullOrEmpty content then
        ProjectSaveState.Empty("grpc")
      else
        Instance.jsonDeserialize<ProjectSaveState> (content)

    saveState

  let updateClientConfig (io: IOResolver) (clientConfigFile: string) (clientConfig: ClientConfig) =
    task {
      let oldClientPath = Path.GetDirectoryName clientConfigFile

      let file, filePath =
        let fileName = Path.GetFileName clientConfigFile
        let oldName = Path.GetDirectoryName clientConfigFile |> Path.GetFileName

        let currentName = clientConfig.Name
        let nameChanged = not (oldName = currentName)

        if nameChanged then
          let newClientPath =
            Path.GetDirectoryName oldClientPath |> fun p -> Path.Combine(p, currentName)

          io.Dir.Move oldClientPath newClientPath

          let newConfigFile = Path.Combine(newClientPath, fileName)
          newConfigFile, newClientPath
        else
          clientConfigFile, oldClientPath

      let json = Instance.jsonSerialize clientConfig
      do! io.File.WriteAllTextAsync file json

      let clientGroup = loadClient io filePath
      GlobalHub.publish (MsgClientUpdated(clientGroup, filePath, oldClientPath))
    }

  let deleteClient (cfg: ClientGroup) (io: IOResolver) =
    io.Dir.Delete cfg.Path true //deletes everything
    io.Log.Info $"Deleted {cfg.Name}"

  let addClient
    (io: IOResolver)
    (project: Project)
    clientName
    serviceName
    protoOrUrl
    description
    (csFiles: string array)
    =
    task {
      //1. Create the client folder
      let clientPath = Path.Combine(project.Path, clientName)
      io.Dir.CreateDirectory clientPath

      let config =
        ClientConfig(Description = description, ServiceName = serviceName, Url = protoOrUrl, Name = clientName)

      let clientConfigFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)

      //2. Copy the C# files
      let codePath = Path.Combine(clientPath, "code")
      io.Dir.CreateDirectory codePath

      for source in csFiles do
        let name = Path.GetFileName source
        let target = Path.Combine(codePath, name)
        io.File.Copy(source, target, true)

      //3. Save the client config
      do! updateClientConfig io clientConfigFile config
    }
