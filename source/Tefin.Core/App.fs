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

    let loadPackage (io: IOResolver) (packagePath: string) =
        let dirs = Directory.GetDirectories(Path.Combine(packagePath, "projects"))
        let projects = dirs |> Array.map (loadProject io)
        let packageName = Path.GetFileName packagePath

        { Name = packageName
          Projects = projects
          Path = packagePath }

    let loadRoot (io: IOResolver) =
        let packages =
            Path.Combine(Root.Path, "packages")
            |> Directory.GetDirectories
            |> Array.map (loadPackage io)

        { Packages = packages
          AppConfigFile = Config.configFile }

    let setupRoot (io: IOResolver) =

        io.Dir.CreateDirectory(Root.Path)
        setupPackage ()

    let init (io) = setupRoot (io)

    let getProject (io: IOResolver) packageName projName =
        let root = loadRoot io

        root.Packages
        |> Array.find (fun c -> c.Name = packageName)
        |> fun p -> p.Projects |> Array.find (fun c -> c.Name = projName)

    let getDefaultProject (io: IOResolver) =
        getProject io defaultPackage Project.DefaultName
