namespace Tefin.Core.Interop

open System.Collections.ObjectModel
open System.Threading.Tasks

open Tefin.Core

type IPackage =
  abstract Name: string
  abstract Init: io: Tefin.Core.IOs -> Task
  abstract GetConfig: unit -> ReadOnlyDictionary<string, string>

[<AutoOpen>]
module AppTypes =
  type AppConfig =
    { AutoSaveFrequency: int }

    static member Default() = { AutoSaveFrequency = 5 }
    static member FileName = "app.config"

  type AppProject =
    { Path: string
      Package: string }

    static member Create path pack = { Path = path; Package = pack }

  type AppState =
    { RecentProjects: AppProject array
      ActiveProject: AppProject }

    static member FileName = "app.saveState"

  type Package =
    { Name: string
      Path: string
      Projects: Project array }

    static member Empty() =
      { Projects = Array.empty
        Name = ""
        Path = "" }

  type Root =
    { Packages: Package array
      AppConfigFile: string }

    static member Path = Utils.getAppDataPath ()

    static member Empty() =
      { Packages = Array.empty
        AppConfigFile = "" }
