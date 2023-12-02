namespace Tefin.Core.Interop

open System
open System.Collections.ObjectModel
open System.Globalization
open System.IO
open System.Reflection
open System.Threading.Tasks

open Tefin.Core

type IPackage =
    abstract Name: string
    abstract Init: unit -> Task
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