namespace Tefin.Core

open System
open System.IO
open Tefin.Core.Interop
open Tefin.Core.Reflection
open Tefin.Core.Project
module App =

    let defaultPackage = "grpc"

    let setupPackage () =
        let packageTypes =
            AppDomain.CurrentDomain.GetAssemblies()
            |> Array.collect (fun a -> a.GetTypes())
            |> Array.filter (fun t -> t.IsClass && TypeHelper.isOfType_<IPackage> (t))

        for p in packageTypes do
            let pack = (Activator.CreateInstance p) :?> IPackage
            let _ = pack.Init()
            ()

    let loadPackage (packagePath: string) =
        let dirs = Directory.GetDirectories(Path.Combine(packagePath, "projects"))
        let projects = dirs |> Array.map (fun d -> loadProject d)
        let packageName = Path.GetFileName packagePath

        { Name = packageName
          Projects = projects
          Path = packagePath }

    let loadRoot () =
        let packages =
            Path.Combine(Root.Path, "packages")
            |> Directory.GetDirectories
            |> Array.map (fun d -> loadPackage d)

        { Packages = packages
          AppConfigFile = Config.configFile }

    let setupRoot () =
        ignore <| Directory.CreateDirectory(Root.Path)
        setupPackage ()

    let init () = setupRoot ()

    let getProject packageName projName =
        let root = loadRoot ()

        root.Packages
        |> Array.find (fun c -> c.Name = packageName)
        |> fun p -> p.Projects |> Array.find (fun c -> c.Name = projName)

    let getDefaultProject () =
        getProject defaultPackage Project.DefaultName