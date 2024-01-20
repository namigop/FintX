namespace Tefin.Core.Interop

open System.Collections.ObjectModel
open System.Threading.Tasks

open Tefin.Core

type IPackage =
    abstract Name: string
    abstract Init : io:Tefin.Core.IOResolver -> Task
    abstract GetConfig: unit -> ReadOnlyDictionary<string, string>

[<AutoOpen>]
module AppTypes =
    type AppConfig = { Todo: string }

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
