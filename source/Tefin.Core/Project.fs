namespace Tefin.Core

open System.Threading.Tasks
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
    let files = getFiles (codePath, "*.*", SearchOption.TopDirectoryOnly)

    { ConfigFile = configFile
      Config = Some config
      CodeFiles = files
      Methods = _loadMethods clientPath createDirectory getDirectories getFiles

      Name = config.Name
      Path = clientPath }

  let loadClient (io: IOResolver) (clientPath: string) =
    _loadClient clientPath io.File.ReadAllText io.Dir.CreateDirectory io.Dir.GetDirectories io.Dir.GetFiles

  let _createSaveState package (projectPath: string) (writeAllText: string -> string -> unit) =
    let state =
      { Package = package
        ClientState = Array.empty }

    let file = Path.Combine(projectPath, ProjectSaveState.FileName)
    let content = Instance.jsonSerialize state
    writeAllText file content

  let createSaveState (io: IOResolver) package (projectPath: string) =
    _createSaveState package projectPath io.File.WriteAllText

  let _getSaveState (projectPath: string) (fileExists: string -> bool) (readAllText: string -> string) =
    let saveState =
      let content =
        Path.Combine(projectPath, ProjectSaveState.FileName)
        |> fun f -> fileExists f, f
        |> fun (exists, f) -> if exists then readAllText f else ""

      if (System.String.IsNullOrEmpty content) then
        ProjectSaveState.Empty("grpc")
      else
        Instance.jsonDeserialize<ProjectSaveState> (content)

    saveState

  let getSaveState (io: IOResolver) (projectPath: string) =
    _getSaveState projectPath io.File.Exists io.File.ReadAllText

  let _loadProject
    (projectPath: string)
    (getFiles: string * string * SearchOption -> string array)
    (readAllText: string -> string)
    (createDirectory: string -> unit)
    (getDirectories: string -> string array)
    (fileExists: string -> bool)
    =
    let clientPaths =
      getFiles (projectPath, ClientGroup.ConfigFilename, SearchOption.AllDirectories)
      |> Array.map Path.GetDirectoryName

    let projSaveState = _getSaveState projectPath fileExists readAllText
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
    _loadProject projectPath io.Dir.GetFiles io.File.ReadAllText io.Dir.CreateDirectory io.Dir.GetDirectories io.File.Exists

  let _updateClientConfig
    (clientConfigFile: string)
    (clientConfig: ClientConfig)
    (moveDirectory: string -> string -> unit)
    (writeAllTextAsync: string -> string -> Task)
    (readAllText: string -> string)
    (createDirectory: string -> unit)
    (getDirectories: string -> string array)
    (getFiles: string * string * SearchOption -> string array)
    =
    task {
      let oldClientPath = Path.GetDirectoryName clientConfigFile

      let file, filePath =
        let fileName = Path.GetFileName clientConfigFile
        let oldDirName = Path.GetDirectoryName clientConfigFile |> Path.GetFileName

        let currentName = clientConfig.Name
        let nameChanged = not (oldDirName = currentName)

        if nameChanged then
          let newClientPath =
            Path.GetDirectoryName oldClientPath |> fun p -> Path.Combine(p, currentName)

          moveDirectory oldClientPath newClientPath

          let newConfigFile = Path.Combine(newClientPath, fileName)
          newConfigFile, newClientPath
        else
          clientConfigFile, oldClientPath

      let json = Instance.jsonSerialize clientConfig
      do! writeAllTextAsync file json

      let clientGroup =
        _loadClient filePath readAllText createDirectory getDirectories getFiles

      GlobalHub.publish (MsgClientUpdated(clientGroup, filePath, oldClientPath))
    }

  let updateClientConfig (io: IOResolver) (clientConfigFile: string) (clientConfig: ClientConfig) =
    task {
      do!
        _updateClientConfig
          clientConfigFile
          clientConfig
          io.Dir.Move
          io.File.WriteAllTextAsync
          io.File.ReadAllText
          io.Dir.CreateDirectory
          io.Dir.GetDirectories
          io.Dir.GetFiles
    }

  let _deleteClient (client: ClientGroup) (dirDelete: string -> bool -> unit) (log: string -> unit) =
    dirDelete client.Path true //deletes everything
    log $"Deleted {client.Name}"

  let deleteClient (client: ClientGroup) (io: IOResolver) =
    _deleteClient client io.Dir.Delete io.Log.Info

  let rec _addClient
    (project: Project)
    clientName
    serviceName
    protoOrUrl
    description
    (csFiles: string array)
    (createDirectory: string -> unit)
    (fileCopy: string * string * bool -> unit)
    (moveDirectory: string -> string -> unit)
    (writeAllTextAsync: string -> string -> Task)
    (readAllText: string -> string)
    (getDirectories: string -> string array)
    (getFiles: string * string * SearchOption -> string array)
    =
    task {
      //1. Create the client folder
      let clientPath = Path.Combine(project.Path, clientName)
      createDirectory clientPath

      let config =
        ClientConfig(Description = description, ServiceName = serviceName, Url = protoOrUrl, Name = clientName)

      let clientConfigFile = Path.Combine(clientPath, ClientGroup.ConfigFilename)

      //2. Copy the C# files
      let codePath = Path.Combine(clientPath, "code")
      createDirectory codePath

      for source in csFiles do
        let name = Path.GetFileName source
        let target = Path.Combine(codePath, name)
        fileCopy (source, target, true)

      //3. Save the client config
      do! _updateClientConfig clientConfigFile config moveDirectory writeAllTextAsync readAllText createDirectory getDirectories getFiles

    }

  let addClient (io: IOResolver) (project: Project) clientName serviceName protoOrUrl description (csFiles: string array) =
    _addClient
      project
      clientName
      serviceName
      protoOrUrl
      description
      csFiles
      io.Dir.CreateDirectory
      io.File.Copy
      io.Dir.Move
      io.File.WriteAllTextAsync
      io.File.ReadAllText
      io.Dir.GetDirectories
      io.Dir.GetFiles
