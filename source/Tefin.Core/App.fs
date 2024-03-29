namespace Tefin.Core

open System
open System.IO
open Tefin.Core.Interop
open Tefin.Core.Reflection
open Tefin.Core.ProjectStructure

module App =

  let defaultPackage = "grpc"

  let setupPackage () =
    let packageTypes =
      AppDomain.CurrentDomain.GetAssemblies()
      |> Array.collect (fun a -> a.GetTypes())
      |> Array.filter (fun t -> t.IsClass && TypeHelper.isOfType_<IPackage> (t))

    for p in packageTypes do
      let pack = (Activator.CreateInstance p) :?> IPackage
      let _ = pack.Init(Resolver.value)
      ()

  let saveAppState (io: IOs) recentProjects activeProject =
    let file = Path.Combine(Root.Path, AppState.FileName)

    let state =
      { ActiveProject = activeProject
        RecentProjects = recentProjects }

    state |> Instance.jsonSerialize |> io.File.WriteAllText file



  let getAppConfig () = AppConfig.Default()
  let getPackagesPath () = Path.Combine(Root.Path, "packages")

  let getDefaultProjectsPath packageName =
    Path.Combine(getPackagesPath (), packageName, "projects")

  let getDefaultProjectPath package =
    Path.Combine(getDefaultProjectsPath package, Project.DefaultName)

  let getAppState (io: IOs) =
    let file = Path.Combine(Root.Path, AppState.FileName)

    if (io.File.Exists file) then
      let state = file |> io.File.ReadAllText |> Instance.jsonDeserialize<AppState>
      let recents = state.RecentProjects |> Array.filter (fun d -> io.Dir.Exists d.Path)
      let activeOpt = recents |> Array.tryFind (fun d -> d.Path = state.ActiveProject.Path)
      match activeOpt with
      | Some a -> { RecentProjects = recents; ActiveProject = a }
      | None -> { RecentProjects = recents
                  ActiveProject = AppProject.Create (getDefaultProjectPath defaultPackage) defaultPackage }
    else
      let path = getDefaultProjectPath defaultPackage

      { RecentProjects = Array.empty
        ActiveProject = AppProject.Create path defaultPackage }

  let loadPackage (io: IOs) (packagePath: string) =
    let dirs = io.Dir.GetDirectories(Path.Combine(packagePath, "projects"))
    let projects = dirs |> Array.map (loadProject io)
    let packageName = Path.GetFileName packagePath

    { Name = packageName
      Projects = projects
      Path = packagePath }

  let loadRoot (io: IOs) =
    let packages =
      getPackagesPath () |> io.Dir.GetDirectories |> Array.map (loadPackage io)

    { Packages = packages
      AppConfigFile = Config.configFile }

  let setupRoot (io: IOs) =
    io.Dir.CreateDirectory(Root.Path)
    setupPackage ()

  let init (io) = setupRoot (io)

  let getProject (io: IOs) packageName projName =
    let root = loadRoot io

    root.Packages
    |> Array.find (fun c -> c.Name = packageName)
    |> fun p -> p.Projects |> Array.find (fun c -> c.Name = projName)

  let getDefaultProject (io: IOs) =
    getProject io defaultPackage Project.DefaultName
