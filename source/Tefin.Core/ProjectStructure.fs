namespace Tefin.Core

open System.Threading.Tasks
open Tefin.Core.Infra.Actors
open Tefin.Core.Interop
open System.IO
open ClientStructure

 
module ProjectStructure =

   
  let _createSaveState package (projectPath: string) (writeAllText: string -> string -> unit) =
    let state =
      { Package = package
        ClientState = Array.empty }

    let file = Path.Combine(projectPath, ProjectSaveState.FileName)
    let content = Instance.jsonSerialize state
    writeAllText file content

  let createSaveState (io: IOs) package (projectPath: string) =
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

  let getSaveState (io: IOs) (projectPath: string) =
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
      Mocks = Array.empty
      Clients = clients
      ConfigFile = config
      Path = projectPath }

  let loadProject (io: IOs) (projectPath: string) =
    let lp = _loadProject projectPath io.Dir.GetFiles io.File.ReadAllText io.Dir.CreateDirectory io.Dir.GetDirectories io.File.Exists
    let mocks =
      io.Dir.GetFiles (projectPath, ServiceMockGroup.ConfigFilename, SearchOption.AllDirectories)
      |> Array.map Path.GetDirectoryName
      |> Array.map (fun path -> ServiceMockStructure.loadServiceMock io path)

    { lp with Mocks = mocks }
    

   