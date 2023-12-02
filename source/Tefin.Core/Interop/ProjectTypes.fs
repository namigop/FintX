namespace Tefin.Core.Interop

open System.Globalization
open System.IO
open Tefin.Core

[<AutoOpen>]
module ProjectTypes =

    //Use a class instead of an F# record to easily serialize to json
    type ClientConfig() =
        member val Name = "" with get, set
        member val ServiceName = "" with get, set
        member val Url = "" with get, set
        member val IsUsingSSL = false with get, set
        member val Jwt = "" with get, set
        member val Description = "" with get, set
        member val IsCertFromFile = false with get, set
        member val CertStoreLocation = "" with get, set
        member val CertThumbprint = "" with get, set

    type ClientGroup =
        { Name: string
          CodeFiles: string array
          ConfigFile: string
          Config: ClientConfig option
          Path: string }

        static member ConfigFilename = "config.json"

        static member Empty() =
            { Name = ""
              CodeFiles = Array.empty
              ConfigFile = ""
              Path = ""
              Config = None }

    type Project =
        { Clients: ClientGroup array
          ConfigFile: string
          Name: string
          Path: string }

        static member ProjectConfigFileName = "projectConfig.json" //TODO
        static member DefaultName = "_default"

        static member Empty() =
            { Clients = Array.empty
              ConfigFile = ""
              Name = ""
              Path = "" }